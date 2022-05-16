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
    public class SearchController : Controller
    {
        FBCloneEntities db = new FBCloneEntities();
        //viewable
        public ActionResult SearchResult(string keyword)
        {
            ViewBag.key = keyword;
            List<User> users = db.Users.Where(n => n.FName.Contains(keyword) || n.Email.Contains(keyword) || n.LName.Contains(keyword) || n.Mobile.Contains(keyword)).ToList();
            return View(users);
        }
        public ActionResult UserProfile(User u)
        {
            Session["OtherUser"] = u.UserId;
            Session["OtherUserName"] = u.FName + " " + u.LName;
            Session["otherUserImg"] = u.ImgUrl;

            int id = (int)Session["UserId"];
            Friend f = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            Friend other = db.Friends.Where(n => n.UserId == u.UserId).FirstOrDefault();
            bool isFriends = false, isReqSent = false; ;
            if (f.FriendId != null)
            {
                string[] friendsId = f.FriendId.Split('#');

                if (f.FriendId != null)
                {
                    friendsId = f.FriendId.Split('#');
                }
                for (int i = 0; i < friendsId.Length; i++)
                {
                    ViewBag.friends += friendsId[i];
                }
                isFriends = Array.Exists(friendsId, s => s.Equals(u.UserId.ToString()));
            }

            if (other.FriendRequests != null)
            {
                string[] friendRequests = other.FriendRequests.Split('#');
                if (other.FriendRequests != null)
                {
                    friendRequests = other.FriendRequests.Split('#');
                }
                for (int i = 0; i < friendRequests.Length; i++)
                {
                    ViewBag.friendreqs += friendRequests[i];
                }
                isReqSent = Array.Exists(friendRequests, s => s.Equals(id.ToString()));
            }

            if (isFriends)
            {
                ViewBag.relation = "fri";
            }
            else if (isReqSent)
            {
                ViewBag.relation = "req";
            }
            else
            {
                ViewBag.relation = "unk";
            }
            return View(u);
        }
        public ActionResult ShowUserProfilePosts()
        {
            int id = (int)Session["OtherUser"];
            List<Post> p = db.Posts.Where(n => n.UserId == id).ToList();
            User u = db.Users.Where(n => n.UserId == id).FirstOrDefault();
            ViewBag.uname = u.FName + " " + u.LName;
            ViewBag.img = u.ImgUrl;
            p.Reverse();
            return PartialView(p);
        }
        //non viewable
        public ActionResult FetchUser(int id)
        {
            User u = db.Users.Where(n => n.UserId == id).FirstOrDefault();
            return RedirectToAction("UserProfile", u);
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