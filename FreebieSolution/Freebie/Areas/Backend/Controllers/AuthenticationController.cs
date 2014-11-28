using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Freebie.Models;
using System.Collections;
using Freebie.Libs;

namespace Freebie.Areas.Backend.Controllers
{
    public class AuthenticationController : Controller
    {
        //
        // GET: /Backend/Login/
        //private EchoContext db = new EchoContext();

        public ActionResult Login()
        {
            FormsAuthentication.SignOut();
            RemoveCoookie("Register");
            RemoveCoookie("freebie");
            Session.Clear();
            return View();
        }

        public ActionResult Logout()
        {
            using (var db = new EchoContext())
            {
                if (Session["User_No"] != null)
                {
                    string user_no = Session["User_No"].ToString();
                    User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));
                    HttpRuntime.Cache.Remove(user_no.Trim());
                    FormsAuthentication.SignOut();
                    RemoveCoookie("freebie");
                    Session.Clear();
                    FreebieEvent.UserEvent(current_user, "A02", Permission.staff_home_page_id);
                }

                return View("Login");
            }
        }

        [HttpPost]
        public ActionResult ValidateUser()
        {
            using (var db = new EchoContext())
            {
                string username = Request.Form["UserName"];
                string password = Request.Form["Password"];
                string enc = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");


                if (username != "" && password != "")
                {
                    User u = db.Users.Where(x => x.User_Name.Equals(username)).Where(x => x.Password.Equals(enc)).SingleOrDefault();
                    if (u != null)
                    {
                        if (u.Status_Cd.Trim().Equals("IA"))
                        {
                            ViewBag.LoginError = System.Configuration.ConfigurationManager.AppSettings["Login002"];
                            return View("Login");
                        }
                        //if (System.Web.HttpContext.Current.Cache[u.User_No.Trim()] == null)
                        //{
                            FormsAuthentication.SetAuthCookie(username, true);
                            Session["User_No"] = u.User_No;
                            Session["Role"] = u.Role_Cd;
                            Session["Dept"] = u.Dept_Cd;
                            Session["Group_Id"] = u.Group_Id;

                            //System.Web.HttpContext.Current.Cache[u.User_No.Trim()] = Session.SessionID;

                            //load permissions
                            IEnumerable<PageMap> page_maps = db.PageMaps.Where(x => x.Role_Cd.Equals(u.Role_Cd)).Where(x => x.Dept_Cd.Equals(u.Dept_Cd)).ToList();
                            Hashtable permissions = new Hashtable();

                            foreach (var p in page_maps)
                            {
                                string page_key = p.Page_Id.ToString();

                                Hashtable item = new Hashtable();
                                item["View_All"] = (p.View_All_Flag.Equals("Y"));
                                item["Access_All"] = (p.Full_Access_Flag.Equals("Y"));
                                item["Allow_Update"] = (p.Allow_Update_Flag.Equals("Y"));
                                permissions.Add(page_key, item);
                            }
                            Session["Permissions"] = permissions;
                            string dept_name = "-";
                            if (u.Dept != null)
                            {
                                dept_name = u.Dept.Dept_Name_En;
                            }
                            FreebieEvent.UserEvent(u, "A01", Permission.staff_home_page_id);
                            return RedirectToAction("StaffProfile", "User");

                        //}
                        //else
                        //{
                        //    ViewBag.LoginError = System.Configuration.ConfigurationManager.AppSettings["MULTIPLE_LOGIN"];
                        //}
                    }
                    else
                    {
                        ViewBag.LoginError = System.Configuration.ConfigurationManager.AppSettings["Login001"];
                    }

                }


                return View("Login");
            }      
        }

        public void AddCookie(string ckname, string[] key, string[] value)
        {
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie == null)
            {
                cookie = new HttpCookie(ckname);
                int i = 0;
                foreach (var k in key)
                {
                    cookie[k] = value[i];
                    i++;
                }

            }
            else
            {
                cookie = HttpCookieEncryption.Decrypt(cookie);
                int i = 0;
                foreach (var k in key)
                {
                    if (cookie[k] == null)
                    { cookie.Values.Add(k, value[i]); }
                    else
                    { cookie[k] = value[i]; }
                    i++;
                }

            }

            HttpCookie cookie_enc = HttpCookieEncryption.Encrypt(cookie);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie_enc);
        }

        public string GetCookie(string ckname, string key)
        {
            string value = "";
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie == null)
            {
                return value;
            }
            cookie = HttpCookieEncryption.Decrypt(cookie);
            value = cookie[key];
            return value;
        }

        public void RemoveCoookie(string ckname)
        {
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }
    }
}
