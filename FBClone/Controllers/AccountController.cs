using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using FBClone.Models;
using Newtonsoft.Json;

namespace FBClone.Controllers
{
    public class AccountController : Controller
    {
        FBCloneEntities db = new FBCloneEntities();
        //viewable
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            if (ModelState.IsValid && user.FName != null && user.LName != null && user.Email != null)
            {
                user.ImgUrl = "DefaultProfile.png";
                db.Users.Add(user);
                db.SaveChanges();
                Friend f = new Friend();
                f.UserId = user.UserId;
                f.FriendId = null;
                f.FriendRequests = null;
                db.Friends.Add(f);
                db.SaveChanges();
                SaveUserSession(user);
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.error = "Something went wrong";
                return View();
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(ViewModel_Login user)
        {

            User u = db.Users.Where(n => n.Email == user.Email).FirstOrDefault();
            if (u != null)
            {
                if (u.Password == user.Password)
                {
                    List<User> users = db.Users.ToList();
                    u.Status = 1;
                    db.SaveChanges();
                    SaveUserSession(u);
                    return RedirectToAction("Home");
                }
                else
                {
                    ViewBag.error = "Incorrect Password";
                }
            }
            else
            {
                ViewBag.error = "User Not Found Wrong Email Address";
            }

            return View();
        }
        public ActionResult SignOut()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult Home()
        {
            Session["posts"] = 1;
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult ShowUserHome()
        {
            Session["posts"] = 2;
            int id = (int)Session["UserId"];
            Friend userRelations = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            if (userRelations.FriendId == null) return View();
            string[] friendsId = userRelations.FriendId.Split('#');
            TempData["friendsUsers"] = db.Users.Where(n => friendsId.Contains(n.UserId.ToString())).ToArray();
            List<Post> posts = db.Posts.Where(n => friendsId.Contains(n.UserId.ToString()) && n.Privacy > 0).ToList();
            posts.Reverse();
            return View(posts);
        }
        public ActionResult ChangeImage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeImage(HttpPostedFileBase img)
        {
            if (img != null)
            {
                img.SaveAs(Server.MapPath("~/UsersMedia/ProfilePictures/" + img.FileName));


                int userid = (int)Session["UserId"];
                User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
                u.ImgUrl = img.FileName;
                db.SaveChanges();
                SaveUserSession(u);

                ViewBag.error = "Success " + userid + " IMG URL " + u.ImgUrl;
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.error = "Null Img";
                return View();
            }

        }
        public ActionResult EditInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult EditInfo(User newUser)
        {
            int id = (int)Session["UserId"];
            User fetchedUser = db.Users.Where(n => n.UserId == id).FirstOrDefault();
            if (newUser.FName != null)
                fetchedUser.FName = newUser.FName;
            if (newUser.LName != null)
                fetchedUser.LName = newUser.LName;
            if (newUser.Email != null)
                fetchedUser.Email = newUser.Email;
            if (newUser.City != null)
                fetchedUser.City = newUser.City;
            if (newUser.Country != null)
                fetchedUser.Country = newUser.Country;
            if (newUser.Mobile != null)
                fetchedUser.Mobile = newUser.Mobile;
            db.SaveChanges();
            SaveUserSession(fetchedUser);
            return RedirectToAction("Home");
        }

        public ActionResult List()
        {
            List<User> users = db.Users.ToList();
            return View(users);
        }

        //HELPING FUNCTIONS --------------------------------
        public void SaveUserSession(User u)
        {
            Session["FName"] = u.FName;
            Session["LName"] = u.LName;
            Session["City"] = u.City;
            Session["Country"] = u.Country;
            Session["Email"] = u.Email;
            Session["Mobile"] = u.Mobile;
            Session["UserId"] = u.UserId;
            Session["Password"] = u.Password;
            Session["UserImg"] = u.ImgUrl;
            Session["PostPrivacy"] = u.Privacy;
        }

    }


}