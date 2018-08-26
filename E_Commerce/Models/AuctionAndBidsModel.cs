using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce.Models
{
    public class AuctionAndBidsModel
    {
        public IEnumerable<bid> bids { get; set; }
        public auction auction { get; set; }
    }
}