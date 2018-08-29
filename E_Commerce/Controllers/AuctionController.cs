using E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using E_Commerce.Hubs;

namespace E_Commerce.Controllers
{
    public class AuctionController : Controller
    {
        // GET: Auction
        private AuctionsHub auctionsHub = new AuctionsHub();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Home(MessageInfo? message)
        {
            log.Info("Auction/Home has been fired.");
            //page with search and add button
            ViewBag.StatusMessage =
               message == MessageInfo.ChangeSuccess ? "Successfully opened a auction."
               : message == MessageInfo.ChangeUnsuccess ? "Error, something went wrong."
               : message == MessageInfo.AuctionFinished ? "Error, the auction is not in the READY state."
               : message == MessageInfo.AuctionClosed ? "Auction has been successfully closed."
                : message == MessageInfo.AuctionOver ? "The auction has finished."
                 : message == MessageInfo.BidSuccess ? "Your bid has been successfully placed."
                 : message == MessageInfo.NoTokens ? "Not enough tokens."
                 : message == MessageInfo.RequiredFields ? "All fields are required."
                 : message == MessageInfo.SuccessCreation ? "Auction successfully created."
               : "";
            return View();
        }

        //open an auction that has been created
        /*
         * I show the list of all auctions that can be opened
         * II btn that changes the state of the auction--->javascript
         * */
        [System.Web.Mvc.Authorize(Roles = "admin")]
        public ActionResult OpenAdmin(int? page)
        {
            log.Info("Auction/OpenAdmin has been fired.");
            using (var context = new E_Commerce.Models.AuctionsDB())
            {
                IEnumerable<E_Commerce.Models.auction> auctions = from auc in context.auction
                                                                  where auc.status == "READY"
                                                                  select auc;
                int pageSize = AdminParams.N;
                int pageNum = 1;
                if (page != null)
                {
                    pageNum = (int)page;
                }

                auctions = auctions.OrderBy(s => s.createdAt);
                return View(auctions.ToPagedList(pageNum, pageSize));
            }
        }
        //TODO
        [System.Web.Mvc.Authorize(Roles ="admin")]
        public ActionResult OpenConfirm(Guid idAukcije)
        {
            log.Info("Auction/OpenConfirm has been fired.");
            if (idAukcije == null)
            {
                log.Error("Auction/Home idAuction is null.");
                return RedirectToAction("Home", new { Message = MessageInfo.ChangeUnsuccess });
            }
            using(var context=new AuctionsDB())
            {
                auction auction = context.auction.Find(idAukcije);
                if (auction == null)
                {
                    return RedirectToAction("Home", new { Message = MessageInfo.ChangeUnsuccess });
                }
                if (!auction.status.Contains("READY"))
                {
                    log.Error("Auction/Home auction is not ready.");
                    return RedirectToAction("Home", new { Message = MessageInfo.AuctionFinished });
                }

                //otvaranje
                auction.status = "OPENED";
                auction.openedAt = DateTime.Now;
                auction.closedAt = DateTime.Now.AddSeconds((double)auction.duration);
                context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                var myHub = GlobalHost.ConnectionManager.GetHubContext<AuctionsHub>();
                myHub.Clients.All.updateStatus(idAukcije);
            }
            return RedirectToAction("Home", new { Message = MessageInfo.ChangeSuccess });
        }
       public void CheckFinish()
        {
            var myHub = GlobalHost.ConnectionManager.GetHubContext<AuctionsHub>();
            //provera da li su zatvorene neke od otvorenih aukcija
            using (var context=new AuctionsDB())
            {
                IEnumerable<auction> auctions= (from a in context.auction
                                                where a.status == "OPENED"
                                                select a).ToList();
                foreach (var auction in auctions)
                {
                    DateTime now = DateTime.Now;
                    DateTime closing = (DateTime)auction.closedAt;
                    if (now > closing)
                    {
                        auction.status = "COMPLETED";
                        context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();                       
                        myHub.Clients.All.closeAuction(auction.id);
                    }
                }
            }
            
        }
        //TODO
        [System.Web.Mvc.Authorize]
        [HttpPost]
        public ActionResult Bid(Guid idAukcije)
        {
            CheckFinish();
            log.Info("Auction/Bid has been fired.");
            int result=auctionsHub.SendBid(idAukcije,User.Identity.GetUserId());
            if (result==1)
            {
                return RedirectToAction("Home", new { Message = MessageInfo.AuctionOver });
            }
            else if (result == 2)
            {
                return RedirectToAction("Home", new { Message = MessageInfo.NoTokens });
            }
            else if (result == 3)
            {
                return RedirectToAction("Home", new { Message = MessageInfo.ChangeUnsuccess });
            }
            else
            {
                return RedirectToAction("Home", new { Message = MessageInfo.BidSuccess });
            }

        }

        [System.Web.Mvc.Authorize]
        [HttpPost]
        public void FinishAuction(Guid id)
        {
            //da li mora actionresult da vrati----check
            log.Info("Auction/FinishAuction has been fired.");
            if (id == null)
            {
                return ;
            }
            using (var context = new AuctionsDB())
            {
                auction auction = context.auction.Find(id);
                if (auction == null)
                {
                    return ;
                }
                if (!auction.status.Contains("OPENED"))
                {
                    return;
                }

                //otvaranje
                auction.status = "COMPLETED";
                context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                var myHub = GlobalHost.ConnectionManager.GetHubContext<AuctionsHub>();
                myHub.Clients.All.closeAuction(id);
            }
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCreate(E_Commerce.Models.auctionNEW model)
        {
            log.Info("Auction/ConfirmCreate has been fired.");
            if (ModelState.IsValid)
            {
                DateTime curr = DateTime.Now;
                byte[] image = null;

                if (model.picture != null)
                {
                    // Convert HttpPostedFileBase to byte array.
                    image = new byte[model.picture.ContentLength];
                    model.picture.InputStream.Read(image, 0, image.Length);
                }

                using (var context = new AuctionsDB())
                {
                    var newAuction = new auction()
                    {
                        id = Guid.NewGuid(),
                        title = model.title,
                        duration = model.duration,
                        startPrice = model.startPrice,
                        currentPrice = model.startPrice,
                        createdAt = curr,
                        status = "READY",
                        picture = image
                    };

                    context.auction.Add(newAuction);
                    context.SaveChanges();

                }
                return RedirectToAction("Home", new { Message = MessageInfo.SuccessCreation });
            }
            return RedirectToAction("Home", new { Message = MessageInfo.RequiredFields });
        }

        [AllowAnonymous]
        public ActionResult IndexGet(int? page)
        {
            log.Info("Auction/IndexGet has been fired.");
            int pageSize = AdminParams.N;
            int pageNum = 1;
            if (page != null)
            {
                pageNum = (int)page;
            }
            using (var cont=new AuctionsDB())
            {
                //??????
                return View("Index",cont.auction.OrderByDescending(a=>a.title).ToPagedList(pageNum,pageSize));
            }
           
        }
        [HttpPost]
        public ActionResult Index(string title, string priceLow, string priceHigh, string status, int? page)
        {
            log.Info("Auction/Index has been fired.");
            CheckFinish();
            //list all opened auctions
            //param search
            bool titleE = false;
            bool priceLowE = false;
            bool priceHIghE = false;
            bool statusE = false;
            bool minmax = false;

            if (!String.IsNullOrEmpty(title)) titleE = true;
            if (!String.IsNullOrEmpty(priceLow)) priceLowE = true;
            if (!String.IsNullOrEmpty(priceHigh)) priceHIghE = true;
            if (!String.IsNullOrEmpty(status)) statusE = true;
            if (priceLowE && priceHIghE) minmax = true;

            using (var context = new E_Commerce.Models.AuctionsDB())
            {
                IEnumerable<E_Commerce.Models.auction> auctions = from auc in context.auction
                                                                  select auc;
                /*
                if (statusE)
                {
                    auctions = auctions.Where(a => a.status == status);
                }*/
                try
                {
                    if (minmax)
                    {
                        auctions = auctions.Where(a => a.currentPrice <= Double.Parse(priceHigh) && a.currentPrice >= Double.Parse(priceLow));
                    }
                    else if (priceLowE)
                    {
                        auctions = auctions.Where(a => a.currentPrice >= Double.Parse(priceLow));
                    }
                    else if (priceHIghE)
                    {
                        auctions = auctions.Where(a => a.currentPrice <= Double.Parse(priceHigh));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing to double.");
                }
                if (titleE)
                {
                    string[] titles = title.Split(' ');
                    foreach (var word in titles)
                    {
                        auctions = auctions.Where(a => a.title.Contains(word));
                    }

                }

                //ne radi!?
                
                if (statusE)
                {
                    
                    switch (status) {
                        case "READY":
                            auctions = auctions.Where(a => a.status=="READY     ");
                            break;
                        case "COMPLETED":
                            auctions = auctions.Where(a => a.status=="COMPLETED ");
                            break;
                        case "OPENED":
                            auctions = auctions.Where(a => a.status=="OPENED    ");
                            break;
                    }
                }
                
                /*
                IEnumerable<E_Commerce.Models.AuctionAndLastBidModel> model=null;
                foreach(var auction in auctions)
                {
                    //model.Add(auction,null);
                }*/
                int pageSize = AdminParams.N;
                int pageNum = 1;
                if (page != null)
                {
                    pageNum = (int)page;
                }

                auctions = auctions.OrderByDescending(s => s.title);
                return View(auctions.ToPagedList(pageNum, pageSize));
            }

        }

       
        [HttpPost]
        public ActionResult Show(Guid idAukcije)
        {
            log.Info("Auction/Show has been fired.");
            auction auction = null;
            using(var context= new AuctionsDB())
            {
                auction = context.auction.Find(idAukcije);
                if (auction == null)
                {
                    return HttpNotFound();
                }

                //dohvati bids i korisnike--izlistaj
                var bids = context.bid
                        .Where(b => b.idAuction.Equals( idAukcije))
                        .OrderByDescending(b => b.numTokens)
                        .Take(20);

                //foreachbid get user info
                IEnumerable<bid> allBids = bids.ToArray();
                var model = new AuctionAndBidsModel { auction = auction, bids = allBids };

                return View(model);
            }
        }

        [System.Web.Mvc.Authorize]
        public ActionResult Won(int? page)
        {
            log.Info("Auction/Show has been fired.");
            int pageSize = AdminParams.N;
            int pageNum = 1;
            if (page != null)
            {
                pageNum = (int)page;
            }
            string user = User.Identity.GetUserId();
            using (var context = new AuctionsDB())
            {
                /*
                var completedAuctions = context.auction
                    .Where(a => a.status == "COMPLETE")
                    .OrderByDescending(a => a.closedAt);*/
                IPagedList<auction> won=(from a in context.auction
                                         join b in context.bid
                                         on a.bidId equals b.id
                                         where a.status=="COMPLETED"
                                         where b.idUser==user
                                         orderby a.closedAt
                                         select a).ToPagedList(pageNum, pageSize);
                return View(won);
            }
               
        }

        public enum MessageInfo
        {
            ChangeSuccess,
            ChangeUnsuccess,
            AuctionFinished,
            AuctionClosed,
            AuctionOver,
            BidSuccess,
            NoTokens,
            RequiredFields,
            SuccessCreation
        }
    }
}
