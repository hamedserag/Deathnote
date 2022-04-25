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
        //USER PROFILE --------------------------------
        public ActionResult Home()
        {
            if(Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        //ADD POST --------------------------------
        public ActionResult Post()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Post(Post p)
        {
            FBCloneEntities db = new FBCloneEntities();
            p.UserId = (int)Session["UserId"];
            p.Date = DateTime.Now;
            p.Privacy = (int)Session["PostPrivacy"];
            db.Posts.Add(p);
            db.SaveChanges();
            return RedirectToAction("ShowPosts");
        }
        public ActionResult PostPrivacy()
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["UserId"];
            User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
            if (u.Privacy == 0)
            {
                u.Privacy = 1;
            }
            else
            {
                u.Privacy = 0;
            }

            db.SaveChanges();
            SaveUserSession(u);

            return RedirectToAction("Home");
        }
        public ActionResult ShowPosts()
        {
            FBCloneEntities db = new FBCloneEntities();
            return View(db.Posts.ToList());
        }
        //REGISTER --------------------------------
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            if (ModelState.IsValid && user.FName != null && user.LName != null && user.Email != null)
            {
                FBCloneEntities db = new FBCloneEntities();
                db.Users.Add(user);
                db.SaveChanges();
                Friend f = new Friend();
                f.UserId = user.UserId;
                f.FriendId = null;
                db.Friends.Add(f);
                SaveUserSession(user);
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.error = "Something went wrong";
                return View();
            }
        }
        //LOGIN USER --------------------------------
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
            if (u.UserId != 0)
            {
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.error = "Something went wrong";
                return View();
            }
            
        }

        //SEARCH AND VIEW OTHER USERS --------------------------------
        public ActionResult Search()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Search(string Keyword)
        {
            FBCloneEntities db = new FBCloneEntities();
            List<User> users = db.Users.ToList();
            User u = new User();
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].FName == Keyword || users[i].LName == Keyword || users[i].Email == Keyword || users[i].Mobile == Keyword)
                {
                    u = users[i];
                }
            }
            return RedirectToAction("UserProfile",u);
        }
        public ActionResult UserProfile(User u)
        {
            Session["OtherUser"] = u.UserId;
            return View(u);
        }
        //FRIENDS --------------------------------
        public ActionResult ShowFriends()
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["UserId"];
            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            if (f.FriendId != null)
            {
                
                string[] friendsId = f.FriendId.Split('#');
                List<User> friendsUser = new List<User>();
                for (int i = 0; i < friendsId.Length; i++)
                {
                    if (friendsId[i] != "")
                    {
                        int userId = Convert.ToInt32(friendsId[i]);
                        User u = db.Users.Where(n => n.UserId == userId).FirstOrDefault();
                        friendsUser.Add(u);
                    }
                }
                return View(friendsUser);
            }
            else
            {
                ViewBag.error = "Add some friends to appear here";
            }
            return View();
        }
        public ActionResult SendReq()
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["OtherUser"];
            User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests += Session["UserId"] + "#";
            db.SaveChanges();
            return RedirectToAction("Home");
        }
        public ActionResult ShowFriendReqs()
        {
            if (Session["FriendRequests"] != null)
            {
                FBCloneEntities db = new FBCloneEntities();
                string tmp = Session["FriendRequests"].ToString();
                string[] friendRequestsIds = tmp.Split('#');
                List<User> friendRequestsUsers = new List<User>();
                for (int i = 0; i < friendRequestsIds.Length; i++)
                {
                    if (friendRequestsIds[i] != "")
                    {
                        int userId = Convert.ToInt32(friendRequestsIds[i]);
                        User u = db.Users.Where(n => n.UserId == userId).FirstOrDefault();
                        friendRequestsUsers.Add(u);
                    }
                }
                return View(friendRequestsUsers);
            }
            else
            {
                ViewBag.error = "No New Friend Requests";
            }
            return View();
        }
        public ActionResult AcceptFriendRequest(int id)
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["UserId"];
            User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests = u.FriendRequests.Replace(id+"#", "");

            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            f.FriendId += id + "#";

            f = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            f.FriendId += userid + "#";

            db.SaveChanges();
            SaveUserSession(u);

            return RedirectToAction("ShowFriendReqs");
        }
        public ActionResult RejectFriendRequest(int id)
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["UserId"];
            User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests = u.FriendRequests.Replace(id + "#", "");

            db.SaveChanges();
            SaveUserSession(u);

            return RedirectToAction("ShowFriendReqs");
        }
        public ActionResult RemoveFriend(int id)
        {
            FBCloneEntities db = new FBCloneEntities();
            int userid = (int)Session["UserId"];
            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            f.FriendId = f.FriendId.Replace(id + "#", "");

            db.SaveChanges();
            return RedirectToAction("ShowFriends");
        }
        //CHANGE IMG --------------------------------
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

                FBCloneEntities db = new FBCloneEntities();
                int userid = (int)Session["UserId"];
                User u = db.Users.Where(n => n.UserId == userid).FirstOrDefault();
                u.ImgUrl = img.FileName;
                db.SaveChanges();
                SaveUserSession(u);

                ViewBag.error = "Success " + userid + " IMG URL "+u.ImgUrl ;
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.error = "Null Img";
                return View();
            }
           
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
            Session["FriendRequests"] = u.FriendRequests;
            Session["UserId"] = u.UserId;
            Session["Password"] = u.Password;
            Session["UserImg"] = u.ImgUrl;
            Session["PostPrivacy"] = u.Privacy;
            Session["TimelineImg"] = u.TimelineUrl;
        }

        //EXTRA FUNCTIONS --------------------------------
        public ActionResult List()
        {
            FBCloneEntities db = new FBCloneEntities();
            List<User> users = db.Users.ToList();
            return View(users);
        }

    }
}