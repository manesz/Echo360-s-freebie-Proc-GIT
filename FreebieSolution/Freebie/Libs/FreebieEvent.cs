using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freebie.Models;

namespace Freebie.Libs
{
    public static class FreebieEvent
    {     
        public static void AddError(Exception e, byte page_id)
        {
            using (var db = new EchoContext())
            {
                string event_action = "A06";
                EventLog error_log = new EventLog();
                error_log.Error_Msg = e.Message;
                error_log.Action_Cd = event_action;
                error_log.Page_Id = page_id;
                db.EventLogs.Add(error_log);
                db.SaveChanges();
            }     
        }

        public static void AddCustomError(string err_str, byte page_id)
        {
            using (var db = new EchoContext())
            {
                string event_action = "A06";
                EventLog error_log = new EventLog();
                error_log.Error_Msg = err_str;
                error_log.Action_Cd = event_action;
                error_log.Page_Id = page_id;
                db.EventLogs.Add(error_log);
                db.SaveChanges();
            }
        }

        // login/logout
        public static void UserEvent(User user, string action, byte? page_id)
        {
            using (var db = new EchoContext())
            {
                string event_action = action;  
                EventLog log = new EventLog();
                log.User_No = user.User_No;
                log.Action_Cd = event_action;
                if (page_id != null)
                {
                    log.Page_Id = page_id;
                }
                db.EventLogs.Add(log);
                db.SaveChanges();
            }
        }

      
        // account login/logout
        public static void AccountEvent(Account account, string action, byte? page_id)
        {

            using (var db = new EchoContext())
            {
                string event_action = action;
                EventLog log = new EventLog();                      
                log.Account_No = account.Account_No;                                               
                log.Action_Cd = event_action;
                if (page_id != null)
                {
                    log.Page_Id = page_id;
                }
                db.EventLogs.Add(log);
                db.SaveChanges();

            }
        }

        public static void AccountCreateEvent(Account new_account, string mobile_no, byte? page_id)
        {
            using (var db = new EchoContext())
            {
                string event_action = "A03";
                string account_no = null;
                string user_no = null;
                if (HttpContext.Current.Session["Account_No"] != null)
                {
                    account_no = HttpContext.Current.Session["Account_No"].ToString();
                }
                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }

                EventLog log = new EventLog();
                log.Action_Cd = event_action;
                log.Identification_Number = new_account.Identification_Number;
                log.Mobile_Number = mobile_no;
                log.Account_Status_Cd = new_account.Status_Cd;
                log.Account_No = new_account.Account_No;
                log.User_No = user_no;
                if (page_id != null)
                {
                    log.Page_Id = page_id;
                }
                db.EventLogs.Add(log);
                db.SaveChanges();
            }
        
        }

        public static void AccountUpdateEvent(Account account, string new_value, string type, byte? page_id)
        {
            using (var db = new EchoContext())
            {
                string event_action = "A04";
                string account_no = null;
                string user_no = null;
                if (HttpContext.Current.Session["Account_No"] != null)
                {
                    account_no = HttpContext.Current.Session["Account_No"].ToString();
                }
                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }


                EventLog log = new EventLog();
                log.Action_Cd = event_action;

                if (type != null)
                {
                    if (type.Equals("Status"))
                    {
                        log.Account_Status_Cd = new_value;
                    }

                    if (type.Equals("Idcard"))
                    {
                        log.Identification_Number = new_value;
                    }
                }
                
                
                log.Account_No = account.Account_No;
                log.User_No = user_no;
                if (page_id != null)
                {
                    log.Page_Id = page_id;
                }
                
                db.EventLogs.Add(log);
                db.SaveChanges();
            
            }
        }

        public static void UserCreateEvent(byte page_id)
        {
            using (var db = new EchoContext())
            {
                string user_no = null;
                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }
                EventLog log = new EventLog();
                log.Action_Cd = "A03";
                log.User_No = user_no;
                log.Page_Id = page_id;

                db.EventLogs.Add(log);
                db.SaveChanges();
            }
        }

        public static void UserUpdateEvent(byte page_id, string action)
        {
            using (var db = new EchoContext())
            {
                string user_no = null;
                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }
                EventLog log = new EventLog();
                log.Action_Cd = action;
                log.User_No = user_no;
                log.Page_Id = page_id;

                db.EventLogs.Add(log);
                db.SaveChanges();
            }
        }

        public static void UpdateMobile(Account account, string mobile_no, string action, byte? page_id)
        {
            using (var db = new EchoContext())
            {
                string account_no = null;
                string user_no = null;
                if (HttpContext.Current.Session["Account_No"] != null)
                {
                    account_no = HttpContext.Current.Session["Account_No"].ToString();
                }
                if (HttpContext.Current.Session["User_No"] != null)
                {
                    user_no = HttpContext.Current.Session["User_No"].ToString();
                }

                string event_action = action;
                EventLog log = new EventLog();
                log.Account_No = account.Account_No;
                log.User_No = user_no;
                log.Mobile_Number = mobile_no;
                log.Action_Cd = event_action;
                if (page_id != null)
                {
                    log.Page_Id = page_id;
                }

                db.EventLogs.Add(log);
                db.SaveChanges();
                
            }
        
        }
    }
}