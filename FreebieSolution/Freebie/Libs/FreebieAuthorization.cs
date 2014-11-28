using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;

namespace Freebie.Libs
{
    public class FreebieAuthorization : System.Web.Mvc.AuthorizeAttribute
    {

        public string Url { get; set; }
        public string Type { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                    HttpContext.Current.Session.Clear();
                    filterContext.HttpContext.Response.Redirect(Url);                
            }
            else
            {

                string account_id = "";
                string user_no = "";

                if (HttpContext.Current.Session["Account_Id"] != null)
                {
                    account_id = HttpContext.Current.Session["Account_Id"].ToString();
                }

                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }

                if (string.IsNullOrWhiteSpace(account_id) && string.IsNullOrWhiteSpace(user_no))
                {
                        FormsAuthentication.SignOut();
                        RemoveCoookie("Register");
                        RemoveCoookie("freebie");
                        HttpContext.Current.Session.Clear();
                        filterContext.HttpContext.Response.Redirect(Url);
                }
                else {
                    if (Type.Equals("Backend"))
                    {
                        if (string.IsNullOrWhiteSpace(user_no))
                        {
                                FormsAuthentication.SignOut();
                                RemoveCoookie("Register");
                                RemoveCoookie("freebie");
                                HttpContext.Current.Session.Clear();
                                filterContext.HttpContext.Response.Redirect(Url);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(account_id))
                        {
                                FormsAuthentication.SignOut();
                                RemoveCoookie("Register");
                                RemoveCoookie("freebie");
                                HttpContext.Current.Session.Clear();
                                filterContext.HttpContext.Response.Redirect(Url);
                        }
                    }
                
                }

                base.OnAuthorization(filterContext);
            }
            

        }

        private void RemoveCoookie(string ckname)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(ckname);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

    }
}