using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KoddiTestApp.Models;

namespace KoddiTestApp.Controllers
{
    public class UserController : Controller
    {
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
                // Email exist already check
                var IsExist = EmailExists(user.EmailID);
                if (IsExist)
                {
                    ModelState.AddModelError("EmailExists", "Email already exists");
                    return View(user);
                }
                // End email exist check

                // Password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                // End password hashing

                // Save data to DB
                using (MyDatabaseEntities db =  new MyDatabaseEntities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    message = "Account successfully created.";
                    Status = true;
                }
                // End save data

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

        // Logout

        [NonAction]
        public bool EmailExists(string emailID)
        {
            using (MyDatabaseEntities db = new MyDatabaseEntities())
            {
                var emailOverlap = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return emailOverlap == null ? false : true; // return false if no overlap (email does not exist)
            }
        }
    }
}