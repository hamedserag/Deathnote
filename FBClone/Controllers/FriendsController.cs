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
    public class FriendsController : Controller
    {
        FBCloneEntities db = new FBCloneEntities();
        //viewable
        public ActionResult ShowFriends()
        {

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
        public ActionResult ShowFriendReqs()
        {
            int userid = (int)Session["UserId"];
            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            if (f.FriendRequests != null)
            {

                string[] friendsId = f.FriendRequests.Split('#');
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
        public ActionResult ShowOnlineFriends()
        {
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
                        User u = db.Users.Where(n => n.UserId == userId && n.Status != 0).FirstOrDefault();
                        friendsUser.Add(u);
                    }
                }
                return View(friendsUser);
            }
            return View();
        }
        //non viewable
        public ActionResult SendReq()
        {

            int userid = (int)Session["OtherUser"];
            Friend u = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests += Session["UserId"] + "#";
            db.SaveChanges();
            return RedirectToAction("Home", "Account");
        }
        public ActionResult AcceptFriendRequest(int id)
        {

            int userid = (int)Session["UserId"];
            Friend u = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests = u.FriendRequests.Replace(id + "#", "");

            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            f.FriendId += id + "#";

            f = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            f.FriendId += userid + "#";

            db.SaveChanges();

            return RedirectToAction("ShowFriendReqs");
        }
        public ActionResult RejectFriendRequest(int id)
        {

            int userid = (int)Session["UserId"];
            Friend u = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests = u.FriendRequests.Replace(id + "#", "");

            db.SaveChanges();

            return RedirectToAction("ShowFriendReqs");
        }
        public ActionResult RemoveFriend(int id)
        {

            int userid = (int)Session["UserId"];
            Friend f = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            f.FriendId = f.FriendId.Replace(id + "#", "");

            f = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            f.FriendId = f.FriendId.Replace(userid + "#", "");

            db.SaveChanges();
            return RedirectToAction("ShowFriends");
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