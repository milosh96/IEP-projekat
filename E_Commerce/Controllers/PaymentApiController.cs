using E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace E_Commerce.Controllers
{
    public class PaymentApiController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: api/PaymentApi
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/PaymentApi/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PaymentApi
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/PaymentApi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/PaymentApi/5
        public void Delete(int id)
        {
        }

        //get za confirmation
        public string Get(string clientid, string status)
        {
            log.Info("Api/Status has been fired");

            using (var context = new AuctionsDB())
            {
                tokenOrder to = context.tokenOrder.Find(Guid.Parse(clientid));
                if (to == null)
                {
                    log.Error("API with wrong token id called");
                    return "failed!";
                }

                User user = context.User.Find(to.idUser);
                if (user != null)
                {
                    if (to.status != "SUBMITTED           ")
                    {
                        log.Error("API already called");
                        return "failed!";
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
                    context.SaveChanges();
                    TokenController.sendMail(user.Email, "Centili payment", "Your payment was successful.");                    
                }
                else
                {
                    log.Error("API user not found.");
                    return "failed!";
                }
            }
            return "success!";
        }
    }
}
