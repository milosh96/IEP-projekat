using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using E_Commerce.Helper;
using Microsoft.AspNet.SignalR.Hubs;

namespace E_Commerce.Hubs
{
    /**
     * server hub koji salje informacije ka klijentima
     */
    
    [HubName("auctionsHub")]
    public class AuctionsHub : Hub
    {

        //funkcije za prikaz aukcija

        public static class mutex { public static string forLock = "key"; }

        public int SendBid(Guid auctionID, string userID)
        {
            int message = 0;
            lock (mutex.forLock)
            {
                string fullName = "";
                string newPrice = "";
                bool noTokens = false;
                double timeRemaining = -1;

                new HelpMethods().BidAuction(auctionID, userID, out fullName, out newPrice, out noTokens, out timeRemaining);

                // Call the updateLastBid method to update auction.
                //check if change happened
                if (noTokens && timeRemaining == -1)
                {
                    message =3;
                }
                else if (noTokens)
                {
                    message = 2;
                }
                else if (timeRemaining == -1)
                {
                    message =1;
                }
                else
                {
                    var myHub = GlobalHost.ConnectionManager.GetHubContext<AuctionsHub>();
                    myHub.Clients.All.updateLastBidAuction(auctionID, fullName, newPrice);
                    //Clients.All.updateLastBidAuction(auctionID,fullName,newPrice);
                }          
            }
            return message;
        }
        /*
        public void ChangeStartPrice(string auctionID, string newPrice)
        {

            new HelpMethods().ChangePrice(Int32.Parse(auctionID), newPrice);

            Clients.All.changeStartPriceAdmin(auctionID, newPrice);
        }*/

        public void ActivateAuction(Guid auctionID, double startCalc)
        {
            DateTime start = DateTime.Now;

            new HelpMethods().OpenAuction(auctionID);

            double remaining = (DateTime.Now - start).TotalSeconds;
            Clients.All.auctionOpened(auctionID, remaining);
        }

        public void ChangeAuctionStatusOver(Guid auctionID)
        {

            string newStatus = new HelpMethods().AuctionOver(auctionID);
            Clients.All.auctionStatusChangedOver(auctionID, newStatus);
        }
    }
}