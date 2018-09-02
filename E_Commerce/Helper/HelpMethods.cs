using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using E_Commerce.Models;

namespace E_Commerce.Helper
{
    public class HelpMethods
    {
        public void BidAuction(Guid auctionID, string userID, out string fullName, out string newPrice, out bool tokens, out double timeRemaining)
        {

            try
            {
                auction auction = null;
                User user = null;
                using (var context = new AuctionsDB())
                {
                    using(var trans = context.Database.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            auction = context.auction.Find(auctionID);
                            user = context.User.Find(userID);


                            if ((auction != null) && (user != null))
                            {
                                //cena da ulozimo jos jedan token
                                int minNext = (int)(auction.currentPrice + AdminParams.T);
                                int userMoney = (int)(user.NumberOfTokens * AdminParams.T);
                                if ((userMoney >= minNext) && (auction.status.Contains("OPENED")))
                                {
                                    tokens = false;
                                    //kako korisnik da bira koliko?
                                    int price = minNext;
                                    int tokensNeeded = (int)Math.Ceiling(minNext / AdminParams.T);
                                    /*
                                    double remaining = ((DateTime)auction.closedAt - DateTime.Now).TotalSeconds;

                                    DateTime newClosed = (DateTime)auction.closedAt;
                                    if (remaining <= 10)
                                    {
                                        double increment = remaining * (-1);
                                        increment += 10;

                                        newClosed = newClosed.AddSeconds(increment);
                                    }

                                    auction.closedAt = newClosed;
                                    timeRemaining =  ((DateTime)auction.closedAt - DateTime.Now).TotalSeconds;
                                    */
                                    timeRemaining = 1;
                                    //umanjujemo za onoliko koliko potrebno tokena
                                    user.NumberOfTokens = user.NumberOfTokens - tokensNeeded;
                                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                                    context.SaveChanges();
                                    bid latestBid = new bid()
                                    {
                                        id = Guid.NewGuid(),
                                        numTokens = tokensNeeded,
                                        placedAt = DateTime.Now,
                                        idUser = userID,
                                        idAuction = auctionID
                                    };
                                    auction.bidId = latestBid.id;
                                    //mozda povecamo za 10 posto?
                                    auction.currentPrice = price;
                                    //? sta je ovo auction.bid.Add(latestBid);

                                    context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                                    context.SaveChanges();

                                    context.bid.Add(latestBid);
                                    context.SaveChanges();
                                    trans.Commit();
                                    fullName = user.FirstName + " " + user.LastName;
                                    newPrice = "" + price;
                                }
                                else
                                {
                                    fullName = newPrice = null;

                                    if (!auction.status.Contains("OPENED"))
                                    {
                                        tokens = false;
                                        timeRemaining = -1;
                                    }
                                    else
                                    {
                                        tokens = true;
                                        timeRemaining = 1;
                                    }
                                }

                            }
                            else
                            {
                                fullName = newPrice = null;
                                tokens = true;
                                timeRemaining = -1;
                                throw new Exception();
                            }
                        }
                        catch(Exception ex)
                        {
                            trans.Rollback();
                            fullName = newPrice = null;
                            tokens = true;
                            timeRemaining = -1;
                        }
                    }
                    
                }
            }
            catch (FormatException)
            {
                fullName = newPrice = null;
                tokens = true;
                timeRemaining = -1;
                Console.WriteLine("Error with bidding.");
            }
        }
        /*
        public void ChangePrice(int? id, string newPrice)
        {

            try
            {
                double doubleValue = Convert.ToDouble(newPrice);
                Aukcija editAukcija;

                using (var context = new AukcijaEntities())
                {
                    editAukcija = context.Aukcijas.Find(id);
                }

                if (editAukcija != null)
                {
                    editAukcija.PocetnaCena = doubleValue;
                    editAukcija.TrenutnaCena = doubleValue;
                }

                using (var context = new AukcijaEntities())
                {
                    context.Entry(editAukcija).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    logger.Error("AUCTION PRICE CHANGE: AuctionID: " + editAukcija.AukcijaID + ", AuctionStatus: " + editAukcija.Status + ", AuctionStartPrice: " + editAukcija.PocetnaCena);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to convert '{0}' to a Double.", newPrice);
            }
            catch (OverflowException)
            {
                Console.WriteLine("'{0}' is outside the range of a Double.", newPrice);
            }

            //return RedirectToAction("Index", "Admin", new { id = id });
        }
        */
        //not used
        public void OpenAuction(Guid? id)
        {
            if (id == null)
            {
                return;
            }

            try
            {
                auction auction=null;

                using (var context = new AuctionsDB())
                {
                    auction = context.auction.Find(id);
                }

                if (auction != null)
                {
                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds((int)auction.duration);

                    auction.status = "OPENED";
                    auction.closedAt = endTime;
                    auction.openedAt = startTime;
                }


                using (var context = new AuctionsDB())
                {
                    context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to open auction");
            }
        }

        public string AuctionOver(Guid id)
        {
            string result = "";
            auction auction=null;

            using (var context = new AuctionsDB())
            {
                auction = context.auction.Find(id);
            }


            if (auction != null)
            {
                if (!auction.status.Equals("OPENED"))
                {
                    result = auction.status;
                }
                else
                {
                    auction.status = "COMPLETE";

                    result = auction.status;

                    using (var context = new AuctionsDB())
                    {
                        context.Entry(auction).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        if (result == "COMPLETE")
                        {

                            /* kako dodati poslednji bid??
                            Bid bid = context.Bids.Find(aukcija.BidID);
                            Korisnik korisnik = context.Korisniks.Find(bid.KorisnikID);

                            korisnik.Aukcijas.Add(auction);
                            context.SaveChanges();
                            */
                        }
                    }
                }

            }

            return result;
        }
    }
}