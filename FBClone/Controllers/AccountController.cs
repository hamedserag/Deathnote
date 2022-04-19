using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FBClone.Models;


namespace FBClone.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Home()
        {
            if(Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult Post()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Post(Post p)
        {
            FBCloneEntities db = new FBCloneEntities();
            db.Posts.Add(p);
            db.SaveChanges();
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            FBCloneEntities db = new FBCloneEntities();
            db.Users.Add(user);
            db.SaveChanges();
            SaveUserSession(user);
            return RedirectToAction("Home");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(ViewModel_Login user)
        {
            FBCloneEntities db = new FBCloneEntities();

            User u = new User();
            List<User> users = db.Users.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                if(users[i].Email == user.Email)
                {
                    u = users[i];
                }
            }
            SaveUserSession(u);
            return RedirectToAction("Home");
        }
        public ActionResult List()
        {
            FBCloneEntities db = new FBCloneEntities();
            List<User> users = db.Users.ToList();
            return View(users);
        }

        public ActionResult ChangeImage()
        {
            return View();
        }

        public void SaveUserSession(User u)
        {
            Session["FName"] = u.FName;
            Session["LName"] = u.LName;
            Session["City"] = u.City;
            Session["Country"] = u.Country;
            Session["Email"] = u.Email;
            Session["Mobile"] = u.Mobile;
            Session["FriendRequests"] = u.FriendRequests;
            Session["UserId"] = u.UserId;
            Session["Password"] = u.Password;
        }
    }
}