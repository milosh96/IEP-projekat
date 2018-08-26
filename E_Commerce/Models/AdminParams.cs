using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce.Models
{
    public class AdminParams
    {
        public static  int N { get; set; } = 10;
        public static int D { get; set; } = 120;
        public static int S { get; set; } = 30;
        public static int G { get; set; } = 50;
        public static int P { get; set; } = 100;
        public static string C { get; set; } = "RSD";
        public static float T { get; set; } = 25;

        public  int __N { get; set; } = 10;
        public  int __D { get; set; } = 120;
        public  int __S { get; set; } = 30;
        public  int __G { get; set; } = 50;
        public  int __P { get; set; } = 100;
        public  string __C { get; set; } = "RSD";
        public  float __T { get; set; } = 25;
    }
}