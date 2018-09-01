using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using E_Commerce.Models;
using Microsoft.AspNet.Identity;
using PagedList;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class TokenController : Controller
    {
        // GET: Token
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            log.Info("Token/Index has been fired.");
            return View();
        }

       public ActionResult Buy()
        {
            log.Info("Token/Buy has been fired.");
            return View();
        }

        [AllowAnonymous]
        public ActionResult StatusCentili(Guid clientid, string status)
        {
            log.Info("Token/StatusCentili has been fired.");
            Guid id = clientid;
            using (var context=new AuctionsDB())
            {
                using(var trans = context.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        tokenOrder to = context.tokenOrder.Find(id);
                        if (to != null)
                        {
                            //menjamo status u ono sto je prosledjeno
                            User user = context.User.Find(to.idUser);
                            if (user != null)
                            {
                                if (to.status != "SUBMITTED           ")
                                {
                                    return RedirectToAction("ListOrders", "Token");
                                }
                                if (status.Equals("success"))
                                {
                                    user.NumberOfTokens += (int)to.numTokens;
                                    to.status = "COMPLETED";
                                }
                                else
                                {
                                    to.status = "CANCELED";
                                }
                                context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                                context.Entry(to).State = System.Data.Entity.EntityState.Modified;
                                sendMail(user.Email, "Centili payment", "Your payment was successful.");
                                context.SaveChanges();
                                trans.Commit();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                        log.Error("Centili payment exception caught");
                    }
                }
                
                

            }
            return RedirectToAction("ListOrders", "Token");
        }

        public static void sendMail(string user, string subject, string body)
        {
            try
            {
                log.Info("Token/sendMail has been fired.");
                MailMessage mail = new MailMessage();
                SmtpClient server = new SmtpClient("smtp.gmail.com");
                server.Port = 587;
                server.Credentials = new System.Net.NetworkCredential("iepprojekat18@gmail.com", "netsignalr");
                server.EnableSsl = true;
                mail.From = new MailAddress("iepprojekat18@gmail.com");
                mail.To.Add(user);
                mail.Subject = subject;
                mail.Body = body;
                server.Send(mail);
            }
            catch (Exception e)
            {
                log.Error("sendMail error with credentials.");
            }
            
        }
        public ActionResult ConfirmBuy(string package)
        {
            log.Info("Token/ConfirmBuy has been fired.");
            int numT = 0;
            string link = "";
            if (package.Equals("1"))
            {
                link = "&package=1";
                numT = AdminParams.S;
            }
            else if (package.Equals("2"))
            {
                link = "&package=2";
                numT = AdminParams.G;
            }
            else
            {
                link = "&package=3";
                numT = AdminParams.P;
            }
            ViewBag.link = link;
            ViewBag.package = package;
            E_Commerce.Models.User user = null;
            using(var con = new E_Commerce.Models.AuctionsDB())
            {
                user = con.User.Find(User.Identity.GetUserId());
                if (user != null)
                {
                    //make token order
                    var to=new tokenOrder()
                    {
                        id=Guid.NewGuid(),
                        numTokens=numT,
                        idUser= User.Identity.GetUserId(),
                        status="SUBMITTED",
                        price=AdminParams.T*numT

                    };
                    con.tokenOrder.Add(to);
                    con.SaveChanges();
                    return View(to);
                }
                else
                {
                    return RedirectToAction("ListOrders", "Token");
                }
            }
            
            
        }
        public ActionResult ListOrders(int? page)
        {
            log.Info("Token/ListOrders has been fired.");
            var userId = User.Identity.GetUserId();

            IEnumerable<E_Commerce.Models.tokenOrder> orders = null;
            using (var context = new E_Commerce.Models.AuctionsDB())
            {
                orders = context.tokenOrder.Where(to => userId.Equals(to.idUser));

                int pageSize = AdminParams.N;
                int pageNum = 1;
                if (page != null)
                {
                    pageNum = (int)page;
                }

                orders = orders.OrderBy(s => s.id);
                return View(orders.ToPagedList(pageNum, pageSize));
            }
        }
       public enum MessageToken
        {
            TokenOrdered,
            Error
        }

       
    }
}
