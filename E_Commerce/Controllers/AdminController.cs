using E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E_Commerce.Controllers
{
    [Authorize(Roles ="admin")]
    public class AdminController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Admin
        public ActionResult Index(MessageChange? message)
        {
            log.Info("Admin/Index has been fired.");
            ViewBag.StatusMessage =
               message == MessageChange.ChangeSuccess ? "Paramater(s) have been successfully changed."
               : message == MessageChange.ChangeUnsuccess ? "Error, insert valid paramaters."
               : "";
            if (User.IsInRole("admin"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        public ActionResult ChangeParams()
        {
            log.Info("Admin/ChangeParams has been fired.");
            if (User.IsInRole("admin"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult SaveChanges(AdminParams model)
        {
            log.Info("Admin/SaveChanges has been fired.");
            int N = model.__N;
            int D = model.__D;
            int S = model.__S;
            int G = model.__G;
            int P = model.__P;
            string C = model.__C;
            if (C == null)
            {
                C = "";
            }
            float T = model.__T;
            bool success = false;

            //URADITI TACNU proveru redosleda
            /*
            if (S != AdminParams.S)
            {
                if (G != AdminParams.G)
                {
                    if (G < S)
                    {
                        return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
                    }
                    if (P != AdminParams.P)
                    {
                        if (P < G)
                        {
                            return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
                        }
                        else
                        {
                            AdminParams.P = P;
                        }
                    }
                    AdminParams.G = G;
                }
                else if (P != AdminParams.P)
                {
                    if (P < S)
                    {
                        return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
                    }
                    AdminParams.P = P;
                }
                AdminParams.S = S;
                success = true;
            }
            else if (G != AdminParams.G)
            {
                if (P != AdminParams.P)
                {
                    if (P < G)
                    {
                        return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
                    }
                    else
                    {
                        AdminParams.P = P;
                    }
                }
                AdminParams.G = G;
                success = true;
            }
            else if (P != AdminParams.P)
            {
                //provera da li je vece od G paketa
                if (P < G)
                {
                    return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
                }
                else
                {
                    AdminParams.P = P;
                }
                success = true;
            }
            */
            if ((S < G) && (G < P))
            {
                if ((P != AdminParams.P)|| (AdminParams.S != S) || (AdminParams.G!=G)){
                    success = true;
                }
                AdminParams.P = P;
                AdminParams.G = G;
                AdminParams.S = S;
            }
            else
            {
                return RedirectToAction("Index", new { Message = MessageChange.ChangeUnsuccess });
            }

            //trebalo bui da je dobro
            if (N != AdminParams.N)
            {
                AdminParams.N = N;
                success = true;
            }
            if (D != AdminParams.D)
            {
                AdminParams.D = D;
                success = true;
            }
            if (T != AdminParams.T)
            {
                AdminParams.T = T;
                success = true;
            }
            if (!AdminParams.C.Equals(C))
            {
                if (!C.Equals(""))
                {
                    AdminParams.C = C;
                    success = true;
                }               
            }
            if (success)
            {
                return RedirectToAction("Index", new { Message = MessageChange.ChangeSuccess });
            }
            else
            {
                return RedirectToAction("Index");
            }
           
        }

        public enum MessageChange
        {
            ChangeSuccess,
            ChangeUnsuccess
        }
    }
}