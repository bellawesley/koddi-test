using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KoddiTestApp.Models;

namespace KoddiTestApp.Controllers
{
    public class HomeController : Controller
    {
        /**
[Authorize]
public ActionResult Index()
{
    return View();
}
        
        //POST submit tweet
        [HttpPost]
        public ActionResult Index(Tweet myTweet)
        {
            if (ModelState.IsValid)
            {
                myTweet.UserName = User.Identity.Name;

                using (MyDatabaseEntities db = new MyDatabaseEntities())
                {
                    db.Tweets.Add(myTweet);
                    db.SaveChanges();
                }
            }

            return View();
        }
    
    **/
    }
}