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
        //USER PROFILE --------------------------------
        public ActionResult Home()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult Post()
        {
            return PartialView("Post");
        }
        //ADD POST --------------------------------
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

            return RedirectToAction("Home");
        }
        public ActionResult ShowPosts()
        {
            int id = (int)Session["UserId"];
            List<Post> p = db.Posts.Where(n => n.UserId == id).ToList();
            p.Reverse();
            return PartialView(p);
        }
        public ActionResult ShowUserHome()
        {
            int id = (int)Session["UserId"];
            Friend userRelations = db.Friends.Where(n => n.UserId == id).FirstOrDefault();
            string[] friendsId = userRelations.FriendId.Split('#');
            TempData["friendsUsers"] = db.Users.Where(n => friendsId.Contains(n.UserId.ToString())).ToArray();
            List<Post> posts = db.Posts.Where(n => friendsId.Contains(n.UserId.ToString()) && n.Privacy > 0).ToList();
            posts.Reverse();
            return View(posts);
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
            return RedirectToAction("Home");
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
            return RedirectToAction("Home");
        }
        public ActionResult PostDetails(int postId)
        {
            Session["postId"] = postId;
            Post p = db.Posts.Where(n => n.PostId == postId).FirstOrDefault();
            return View(p);
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
        public ActionResult Comment(string cmt)
        {
            Comment comment = new Comment();
            comment.Date = DateTime.Now;
            comment.Details = cmt;
            comment.PostId = Session["postId"].ToString();
            comment.UserId = Session["UserId"].ToString();
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("PostDetails", comment.PostId);
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
        //LOGIN USER --------------------------------
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
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(ViewModel_Login user)
        {

            User u = db.Users.Where(n => n.Email == user.Email).FirstOrDefault();
            List<User> users = db.Users.ToList();
            u.Status = 1;
            db.SaveChanges();
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
        public ActionResult SignOut()
        {
            int id = (int)Session["UserId"];
            User u = db.Users.Where(n => n.UserId == id).FirstOrDefault();
            u.Status = 0;
            db.SaveChanges();
            Session.Contents.RemoveAll();
            return RedirectToAction("Login");
        }
        //SEARCH AND VIEW OTHER USERS --------------------------------
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
            p.Reverse();
            return PartialView("ShowPosts", p);
        }
        public ActionResult FetchUser(int id)
        {
            User u = db.Users.Where(n => n.UserId == id).FirstOrDefault();
            return RedirectToAction("UserProfile", u);
        }
        //FRIENDS --------------------------------
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
        public ActionResult SendReq()
        {

            int userid = (int)Session["OtherUser"];
            Friend u = db.Friends.Where(n => n.UserId == userid).FirstOrDefault();
            u.FriendRequests += Session["UserId"] + "#";
            db.SaveChanges();
            return RedirectToAction("Home");
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
            fetchedUser.FName = newUser.FName;
            fetchedUser.LName = newUser.LName;
            fetchedUser.Email = newUser.Email;
            fetchedUser.City = newUser.City;
            fetchedUser.Country = newUser.Country;
            fetchedUser.Mobile = newUser.Mobile;
            db.SaveChanges();
            SaveUserSession(fetchedUser);
            return RedirectToAction("Home");
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

        //EXTRA FUNCTIONS --------------------------------
        public ActionResult List()
        {

            List<User> users = db.Users.ToList();
            return View(users);
        }

        public class HttpParamActionAttribute : ActionNameSelectorAttribute
        {
            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                if (actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (!actionName.Equals("Action", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                var request = controllerContext.RequestContext.HttpContext.Request;
                return request[methodInfo.Name] != null;
            }
        }

    }


}