﻿using System;
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
        // Declare frequented database
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
                if (emailOverlap != null) // user exists and can login!
                {
                    if (string.Compare(Crypto.Hash(userLogin.Password), emailOverlap.Password) == 0) // password also valid
                    {
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
                    else // password invalid
                    {
                        message = "Invalid credential provided";
                    }

                }
                else // user DNE
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View(); 
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        
        // POST submit tweet
        [HttpPost]
        public ActionResult Index(Tweet myTweet)
        {
            if (ModelState.IsValid)
            {
                myTweet.UserName = User.Identity.Name;

                using (db)
                {
                    db.Tweets.Add(myTweet);
                    db.SaveChanges();
                }
                ModelState.Clear();
                return View();
            }
            return View(myTweet);
        }

        // See tweets
        [Authorize]
        [HttpPost]
        public ActionResult Log()
        {
            if (ModelState.IsValid)
            {
                return View(db.Tweets.ToList().Where(a => a.UserName == User.Identity.Name).Reverse());
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        // Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }
        

        [NonAction]
        public bool EmailExists(string emailID)
        {
            using (db)
            {
                var emailOverlap = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return emailOverlap == null ? false : true; // return false if no overlap (email does not exist)
            }
        }
    }
}