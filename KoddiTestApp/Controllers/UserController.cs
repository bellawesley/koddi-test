using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KoddiTestApp.Models;
using System.Net;
using System.Web.Security;

namespace KoddiTestApp.Controllers
{
    public class UserController : Controller
    {
        // Declare frequented database for reuse
        MyDatabaseEntities db = new MyDatabaseEntities();

        // Registration action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        // Registration POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(User user)
        {
            bool Status = false;
            string message = "";

            // Model Validation
            if (ModelState.IsValid)
            {
                // Check if email exists already
                var IsExist = EmailExists(user.EmailID);
                if (IsExist)
                {
                    ModelState.AddModelError("EmailExists", "Email already exists");
                    return View(user);
                }

                // Password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                MyDatabaseEntities db = new MyDatabaseEntities();
                // Save data to database if applicable
                using (db)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    message = " Account successfully created.";
                    Status = true;
                }
            }
            else
            {
                message = "Invalid request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;

         return View(user);
        }

        // Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Login POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin userLogin, string ReturnUrl="")
        {
            string message = "";
            using(db)
            {
                var emailOverlap = db.Users.Where(a => a.EmailID == userLogin.EmailID).FirstOrDefault();
                if (emailOverlap != null) // User exists in data
                {
                    if (string.Compare(Crypto.Hash(userLogin.Password), emailOverlap.Password) == 0) // Password valid
                    {
                        // Create ticket for user authentication. Timeout = 30 minutes
                        var ticket = new FormsAuthenticationTicket(userLogin.EmailID, false, 30);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(30);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                        
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "User");
                        } 
                    }
                    else // Password invalid
                    {
                        message = "Invalid credential provided";
                    }

                }
                else // User DNE
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View(); 
        }

        // Tweet portal
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        
        // POST Tweet posting
        [HttpPost]
        public ActionResult Index(Tweet myTweet)
        {
            if (ModelState.IsValid)
            {
                myTweet.UserName = User.Identity.Name; // Each tweet is saved in DB with current user ID

                using (db)
                {
                    db.Tweets.Add(myTweet); // Add tweet to DB
                    db.SaveChanges();
                }
                ModelState.Clear(); // Clear textbox if tweet met criteria and was added to DB
                return View();
            }
            return View(myTweet);
        }

        // POST View tweets
        [Authorize]
        [HttpPost]
        public ActionResult Log()
        {
            if (ModelState.IsValid) // List is produced based on current user. Most recent tweets at top
            {
                return View(db.Tweets.ToList().Where(a => a.UserName == User.Identity.Name).Reverse());
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        // POST Logout action
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }
        
        // Email pre-existance method
        [NonAction]
        public bool EmailExists(string emailID)
        {
            using (db)
            {
                var emailOverlap = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return emailOverlap == null ? false : true; // Return false if no overlap (email does not exist)
            }
        }
    }
}