using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace E_Commerce.Models
{
    public class auctionNEW
    {
        [Required(ErrorMessage = "Title is required.")]
        public string title { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        public int duration { get; set; }

        [Required(ErrorMessage = "Starting price is required.")]
        public double startPrice { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [NotMapped]
        public HttpPostedFileBase picture { get; set; }
    }
}