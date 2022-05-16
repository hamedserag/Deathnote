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
    public class PostController : Controller
    {
        FBCloneEntities db = new FBCloneEntities();
        //viewable
        public ActionResult Post()
        {
            return PartialView("Post");
        }
        [HttpPost]
        public ActionResult Post(Post p)
        {

            if (p.Details != "")
            {
                p.UserId = (int)Session["UserId"];
                p.Date = DateTime.Now;
                p.Privacy = (int)Session["PostPrivacy"];
                p.DislikesId = "#";
                p.LikesId = "#";
                db.Posts.Add(p);
                db.SaveChanges();
            }
            return View("Post");
        }
        public ActionResult CommentSection()
        {
            string id = Session["postId"].ToString();
            List<Comment> cmts = db.Comments.Where(n => n.PostId == id).ToList();
            cmts.Reverse();
            List<string> userIds = new List<string>();
            for (int i = 0; i < cmts.Count; i++)
            {
                userIds.Add(cmts[i].UserId);
            }
            TempData["commentingUsers"] = db.Users.Where(n => userIds.Contains(n.UserId.ToString())).ToArray();
            return View(cmts);
        }
        public ActionResult ShowPosts()
        {
            int id = (int)Session["UserId"];
            List<Post> p = db.Posts.Where(n => n.UserId == id).ToList();
            p.Reverse();
            return PartialView(p);
        }
        public ActionResult PostDetails(int postId)
        {
            Session["postId"] = postId;
            Post p = db.Posts.Where(n => n.PostId == postId).FirstOrDefault();
            User u = db.Users.Where(n => n.UserId == p.UserId).FirstOrDefault();
            ViewBag.uimg = u.ImgUrl;
            ViewBag.uname = u.FName + " " + u.LName;
            return View(p);
        }
        //non viewable
        public ActionResult PostPrivacy()
        {

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

            return RedirectToAction("Home", "Account");
        }
        public ActionResult LikePost(int postId)
        {
            Post p = db.Posts.Where(n => n.PostId == postId).FirstOrDefault();
            string id = Session["UserId"] + "#";
            if (!p.LikesId.Contains(id))
            {
                p.Likes++;
                p.LikesId += id;
            }
            else
            {
                p.Likes--;
                p.LikesId = p.LikesId.Replace(id, "");
            }
            db.SaveChanges();
            if ((int)Session["posts"] == 1) return RedirectToAction("Home", "Account");
            return RedirectToAction("ShowUserHome", "Account");
        }
        public ActionResult DislikePost(int postId)
        {
            Post p = db.Posts.Where(n => n.PostId == postId).FirstOrDefault();
            string id = Session["UserId"] + "#";
            if (!p.DislikesId.Contains(id))
            {
                p.Dislikes++;
                p.DislikesId += id;
            }
            else
            {
                p.Dislikes--;
                p.DislikesId = p.DislikesId.Replace(id, "");
            }
            db.SaveChanges();
            if ((int)Session["posts"] == 1) return RedirectToAction("Home", "Account");
            return RedirectToAction("ShowUserHome", "Account");
        }
        public ActionResult Comment(string cmt)
        {
            Comment comment = new Comment();
            comment.Date = DateTime.Now;
            comment.Details = cmt;
            comment.PostId = Session["postId"].ToString();
            comment.UserId = Session["UserId"].ToString();
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("PostDetails","Post", new { postId = comment.PostId });
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