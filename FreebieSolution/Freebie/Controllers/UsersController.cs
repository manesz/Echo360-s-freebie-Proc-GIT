using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Freebie.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Configuration;
using Freebie.Libs;
using Freebie.Mailers;
using Mvc.Mailer;
using System.Data;
using System.Net;
using System.IO;
using Freebie.ViewModels;
using System.Data.SqlClient;
using System.Collections;
using System.Text;
using System.Transactions;
using System.Data.Objects;
using System.Threading;

namespace Freebie.Controllers
{
    public class UsersController : Controller
    {
        //
        // GET: /Users/
		//private EchoContext db = new EchoContext();

		int current_year = DateTime.Now.Year;
		List<SelectListItem> years = new List<SelectListItem>();
		List<SelectListItem> months = new List<SelectListItem>();
		List<SelectListItem> days = new List<SelectListItem>();
        List<SelectListItem> child1_years = new List<SelectListItem>();
        List<SelectListItem> child2_years = new List<SelectListItem>();
        List<SelectListItem> child3_years = new List<SelectListItem>();
		[HttpGet]
        public ActionResult Login()
        {
            FormsAuthentication.SignOut();
            RemoveCoookie("Register");
            RemoveCoookie("freebie");
            Session.Clear();
            return View();
        }


        //public ActionResult Test()
        //{

        //    var db = new EchoContext();

        //    System.Diagnostics.Debug.WriteLine("test");
        //    //ActivationSMS.seed_data(9);
        //    //ActivationSMS.send_sms();
        //    ActivationSMS.send_sms();
        //    Debug.WriteLine("-------------------------------------------");
        //    return View("Login");
        //}

        //public ActionResult Test(string pn)
        //{
        //    ViewBag.Type = 5;
        //    return View("RenderResult");
        //}
		public ActionResult Logout()
		{
            //EchoContext db = new EchoContext();
            if (Session["Account_Id"] != null)
            {
                using (var db = new EchoContext())
                {
                    int account_id = Convert.ToInt32(Session["Account_Id"].ToString());
                    Account account = db.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
                    HttpRuntime.Cache.Remove(account_id.ToString());
                    FormsAuthentication.SignOut();
                    FreebieEvent.AccountEvent(account, "A02", Permission.f_user_home_page_id);
                }
            }
			return View("Login");
		}

		[HttpPost]
		public ActionResult ValidateUser()
		{
            //EchoContext db = new EchoContext();
			string username = Request.Form["UserName"];
			string password = Request.Form["Password"];
			string enc = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");


			if (username != "" && password != "")
			{
                using (var db = new EchoContext())
                {
                    Account a = db.Accounts.Where(x => x.User_Name.Equals(username)).Where(x => x.Password.Equals(enc)).SingleOrDefault();

                    if (a != null)
                    {
                        //if (System.Web.HttpContext.Current.Cache[a.Account_Id.ToString()] == null)
                        //{
                            FormsAuthentication.SetAuthCookie(username, true);
                            Session["Account_Id"] = a.Account_Id;
                            Session["Account_No"] = a.Account_No;

                            //System.Web.HttpContext.Current.Cache[a.Account_Id.ToString()] = Session.SessionID;

                            FreebieEvent.AccountEvent(a, "A01", Permission.f_user_home_page_id);
                            return RedirectToAction("Index", "AccInfo");
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
				
			}
			
            
			return View("Login");
		}

		[HttpPost]
		public ActionResult VerifySubrNumber()
		{
            EchoContext db = new EchoContext();
			string phoneNumber = Request.Form["PhoneNumber"];
			ViewBag.Step = 1;
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.Path = Url.Content("~/"); 
            int result = CustomValidate.ValidateNumber(phoneNumber);
            ViewBag.ValidNumber = false;
            string otp = "";
            Hashtable quotas = new Hashtable();
            quotas["low"] = new Hashtable();
            quotas["medium"] = new Hashtable();
            quotas["high"] = new Hashtable();
            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
            int q_count = 1;
            foreach (var q in base_quotas)
            {
                switch (q_count)
                {
                    case 1:
                        quotas["low"] = q;
                        break;
                    case 2:
                        quotas["medium"] = q;
                        break;
                    case 3:
                        quotas["high"] = q;
                        break;
                    default:
                        break;
                }

                q_count += 1;
            }
            ViewBag.Quotas = quotas;
            switch (result)
            { 
                case 0:
                    ViewBag.Type = 1;
                    ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account010"]);
                    return View("RegisterByCust");
                case 1:
                    ViewBag.ValidNumber = true;
                    otp = OTPHandler.SendOTPReg(phoneNumber);
                    ViewBag.ShowPwd = true;
                    if (otp.Equals("limit_daily"))
                    {
                        string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp01"];
                        err_str = err_str.Replace("{count}", System.Configuration.ConfigurationManager.AppSettings["OTP_ALLOW_PER_DAY_PER_NUMBER"]);
                        ViewBag.ErrorOTP = err_str;

                    }
                    else
                    {
                        if (otp.Equals("limit_interval"))
                        {
                            string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp02"];
                            err_str = err_str.Replace("{minutes}", System.Configuration.ConfigurationManager.AppSettings["INTERVAL_PERIOD_BETWEEN_OTP"]);
                            ViewBag.ErrorOTP = err_str;
                        }
                    }

                    ViewBag.OTP = otp;                  
                    AddCookie("Register", new string[] {"pn"},  new string[] {phoneNumber});
                    return View("RegisterByCust");
                case 2:
                    ViewBag.Type = 2;
                    return View("RenderStatics");
                case 3:
                    ViewBag.Type = 2;
                    return View("RenderStatics");
                case 4:
                    ViewBag.Type = 2;
                    return View("RenderStatics");
                case 5:
                    ViewBag.ValidNumber = true;
                    otp = OTPHandler.SendOTPReg(phoneNumber);
                    ViewBag.ShowPwd = true;
                    if (otp.Equals("limit_daily"))
                    {
                        string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp01"];
                        err_str = err_str.Replace("{count}", System.Configuration.ConfigurationManager.AppSettings["OTP_ALLOW_PER_DAY_PER_NUMBER"]);
                        ViewBag.ErrorOTP = err_str;
                    }
                    else
                    {
                        if (otp.Equals("limit_interval"))
                        {
                            string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp02"];
                            err_str = err_str.Replace("{minutes}", System.Configuration.ConfigurationManager.AppSettings["INTERVAL_PERIOD_BETWEEN_OTP"]);
                            ViewBag.ErrorOTP = err_str;
                        }
                    }

                    ViewBag.OTP = otp;      
                    AddCookie("Register", new string[] { "pn" }, new string[] { phoneNumber });
                    return View("RegisterByCust");
                case 6:
                    ViewBag.Type = 3;
                    return View("RenderStatics");
                case 7:
                    ViewBag.ValidNumber = true;
                    otp = OTPHandler.SendOTPReg(phoneNumber);
                    ViewBag.ShowPwd = true;
                    if (otp.Equals("limit_daily"))
                    {
                        string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp01"];
                        err_str = err_str.Replace("{count}", System.Configuration.ConfigurationManager.AppSettings["OTP_ALLOW_PER_DAY_PER_NUMBER"]);
                        ViewBag.ErrorOTP = err_str;
                    }
                    else
                    {
                        if (otp.Equals("limit_interval"))
                        {
                            string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp02"];
                            err_str = err_str.Replace("{minutes}", System.Configuration.ConfigurationManager.AppSettings["INTERVAL_PERIOD_BETWEEN_OTP"]);
                            ViewBag.ErrorOTP = err_str;
                        }
                    }

                    ViewBag.OTP = otp;      
                    AddCookie("Register", new string[] { "pn" }, new string[] { phoneNumber });
                    return View("RegisterByCust");
                default:
                    ViewBag.Type = 1;
                    return View("RegisterByCust");
            
            }		
		}

        public ActionResult RegisterByCust()
        {
            ViewBag.Step = 1;
            ViewBag.ValidNumber = false;
            RemoveCoookie("Register");
            Hashtable quotas = new Hashtable();
            quotas["low"] = new Hashtable();
            quotas["medium"] = new Hashtable();
            quotas["high"] = new Hashtable();
            var db = new EchoContext();
            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
            int q_count = 1;
            foreach (var q in base_quotas)
            {
                switch (q_count)
                {
                    case 1:
                        quotas["low"] = q;
                        break;
                    case 2:
                        quotas["medium"] = q;
                        break;
                    case 3:
                        quotas["high"] = q;
                        break;
                    default:
                        break;
                }

                q_count += 1;
            }
            ViewBag.Quotas = quotas;
            return View();
        }

        [HttpPost]
        public ActionResult RegisterByCust(string phone_number)
        {
            EchoContext db = new EchoContext();
            string otp = Request.Form["Password"];
            phone_number = GetCookie("Register", "pn");
            bool flag = true;
            if (string.IsNullOrEmpty(phone_number))
            {
                ViewBag.ValidNumber = false;
                flag = false;
            }
            ViewBag.PhoneNumber = phone_number;
            Hashtable quotas = new Hashtable();
            quotas["low"] = new Hashtable();
            quotas["medium"] = new Hashtable();
            quotas["high"] = new Hashtable();
            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
            int q_count = 1;
            foreach (var q in base_quotas)
            {
                switch (q_count)
                {
                    case 1:
                        quotas["low"] = q;
                        break;
                    case 2:
                        quotas["medium"] = q;
                        break;
                    case 3:
                        quotas["high"] = q;
                        break;
                    default:
                        break;
                }

                q_count += 1;
            }
            ViewBag.Quotas = quotas;
            if (flag && (string.IsNullOrEmpty(otp) || otp.Length < 4))
            {
                ViewBag.ValidNumber = true;
                ViewBag.Error = true;
                ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate010"];
                flag = false;
            }

            
            if (flag)
            {
                int result = OTPHandler.ValidateOTP(phone_number, otp);
                switch (result)
                { 
                    case 0:
                        string inactive = FreebieStatus.MobileInActive();
                        AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phone_number)).Where(x => !x.Status_Cd.Equals(inactive)).SingleOrDefault();
                        if (am != null)
                        {
                            Account ac = db.Accounts.SingleOrDefault(x => x.Account_Id == am.Account_Id);
                            if (ac != null)
                            {
                                AddCookie("Register", new string[] { "aid", "otp" }, new string[] { ac.Account_Id.ToString(), "y" });
                            }
                        }
                        else
                        {
                            AddCookie("Register", new string[] { "otp" }, new string[] { "y" });
                        }
                        return RedirectToAction("RegisterUsername");
                    case 1:
                        ViewBag.Error = true;
                        ViewBag.ValidNumber = true;
                        ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate007"];
                        ViewBag.ShowPwd = true;
                        break;
                    case 2:
                        ViewBag.ValidNumber = false;
                        ViewBag.PhoneNumber = "";
                        ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp03"];
                        RemoveCoookie("Register");
                        break;
                    case 3:
                        ViewBag.ValidNumber = false;
                        ViewBag.PhoneNumber = "";
                        ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp04"];
                        RemoveCoookie("Register");
                        break;
                    default:
                        break;
                }
            }
            ViewBag.Step = 1;
            ViewBag.ShowPwd = true;
            return View();
        }

		public ActionResult RegisterUsername()
		{
            EchoContext db = new EchoContext();
            Account ac = new Account();
            if (GetCookie("Register", "aid") != null)
            {
                int account_id = Convert.ToInt32(GetCookie("Register", "aid"));
                ac = db.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
            }
            string phone_number = GetCookie("Register", "pn");
            string otp_pass = GetCookie("Register", "otp");

            if (string.IsNullOrEmpty(phone_number) || string.IsNullOrEmpty(otp_pass) || (!string.IsNullOrEmpty(otp_pass) && !otp_pass.Equals("y")))
            {
                return RedirectToAction("RegisterByCust");
            }
			ViewBag.Step = 2;
            Hashtable quotas = new Hashtable();
            quotas["low"] = new Hashtable();
            quotas["medium"] = new Hashtable();
            quotas["high"] = new Hashtable();
            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
            int q_count = 1;
            foreach (var q in base_quotas)
            {
                switch (q_count)
                {
                    case 1:
                        quotas["low"] = q;
                        break;
                    case 2:
                        quotas["medium"] = q;
                        break;
                    case 3:
                        quotas["high"] = q;
                        break;
                    default:
                        break;
                }

                q_count += 1;
            }
            ViewBag.Quotas = quotas;
			return View(ac);
		}

        [HttpPost]
        public ActionResult RegisterUsername(string state)
        {
            EchoContext db = new EchoContext();
            Account ac = new Account();

            
            if (GetCookie("Register", "aid") != null)
            {
                int account_id = Convert.ToInt32(GetCookie("Register", "aid"));
                ac = db.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
            }
            Hashtable quotas = new Hashtable();
            quotas["low"] = new Hashtable();
            quotas["medium"] = new Hashtable();
            quotas["high"] = new Hashtable();
            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
            int q_count = 1;
            foreach (var q in base_quotas)
            {
                switch (q_count)
                {
                    case 1:
                        quotas["low"] = q;
                        break;
                    case 2:
                        quotas["medium"] = q;
                        break;
                    case 3:
                        quotas["high"] = q;
                        break;
                    default:
                        break;
                }

                q_count += 1;
            }
            ViewBag.Quotas = quotas;
            var username = Request.Form["Email"];
            var password = Request.Form["Password"];
            var confirm_username = Request.Form["ConfirmEmail"];
            var confirm_password = Request.Form["ConfirmPassword"];
            ViewBag.Step = 2;

            int result = CustomValidate.ValidateUsername(username, confirm_username, password, confirm_password);

            if (result != 1)
            {
                ViewBag.Email = username;
                ViewBag.ConfirmEmail = confirm_username;
                ViewBag.Password = password;
                ViewBag.ConfirmPassword = confirm_password;


                return View(); 
            }
          
            password = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
            AddCookie("Register", new string[] {"email", "pwd"}, new string[] {username, password});
            

            return RedirectToAction("RegisterProfile");
        }

		public ActionResult RegisterProfile()
		{
            using (var db = new EchoContext())
            {
                string phone_number = GetCookie("Register", "pn");
                string otp_pass = GetCookie("Register", "otp");
                if (string.IsNullOrEmpty(phone_number) || string.IsNullOrEmpty(otp_pass) || (!string.IsNullOrEmpty(otp_pass) && !otp_pass.Equals("y")))
                {
                    return RedirectToAction("RegisterByCust");
                }

                string username = GetCookie("Register", "email");
                string password = GetCookie("Register", "pwd");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return RedirectToAction("RegisterUsername");
                }

                Hashtable quotas = new Hashtable();
                quotas["low"] = new Hashtable();
                quotas["medium"] = new Hashtable();
                quotas["high"] = new Hashtable();

                IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);

                int q_count = 1;
                foreach (var quota in base_quotas)
                {
                    switch (q_count)
                    {
                        case 1:
                            quotas["low"] = quota;
                            break;
                        case 2:
                            quotas["medium"] = quota;
                            break;
                        case 3:
                            quotas["high"] = quota;
                            break;
                        default:
                            break;
                    }

                    q_count += 1;
                }
                ViewBag.Quotas = quotas;
                Account ac = new Account();
                List<string> interest_arrs = new List<string>();

                if (GetCookie("Register", "aid") != null)
                {
                    int account_id = Convert.ToInt32(GetCookie("Register", "aid"));
                    ac = db.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
                    var account_interest = db.AccountInterests.Where(x => x.Account_Id.Equals(ac.Account_Id)).SingleOrDefault();

                    if (account_interest == null)
                    {
                        account_interest = new AccountInterest();
                    }

                    interest_arrs = load_interest(account_interest);

                }

                init_dropdown(ac);
                ViewBag.InterestSelected = interest_arrs;
                ViewBag.Step = 3;
                return View(ac);
            }
		}

        [HttpPost]
        public ActionResult CreateAccount(Account ac, string state)
        {
            #region get infomations and variables               
                string phone_number = GetCookie("Register", "pn");
                string username = GetCookie("Register", "email");
                string password = GetCookie("Register", "pwd");
                bool edited = false;
                int return_type = 0;
                DateTime timestamp = DateTime.Now;

                string[] result_sp = new string[2];
                
                
                Hashtable quotas = new Hashtable();
                
                var selected_interests = Request.Form["selectedInterests"];
                var agree_flag = Request.Form["Agree"];

                if (string.IsNullOrWhiteSpace(agree_flag))
                {
                    agree_flag = "";
                }
  
                #region init data
                    // quotas form
                    quotas["low"] = new Hashtable();
                    quotas["medium"] = new Hashtable();
                    quotas["high"] = new Hashtable();
                    int q_count = 1;
                    using (var db = new EchoContext())
                    {
                        IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
                        foreach (var quota in base_quotas)
                        {
                            switch (q_count)
                            {
                                case 1:
                                    quotas["low"] = quota;
                                    break;
                                case 2:
                                    quotas["medium"] = quota;
                                    break;
                                case 3:
                                    quotas["high"] = quota;
                                    break;
                                default:
                                    break;
                            }

                            q_count += 1;
                        }
                    }
                    // interests
                    List<string> interest_arrs = new List<string>();
                    var account_interest = new AccountInterest();

                    using (var db = new EchoContext())
                    {
                        account_interest = db.AccountInterests.Where(x => x.Account_Id.Equals(ac.Account_Id)).SingleOrDefault();
                    }
                    if (account_interest == null)
                    {
                        account_interest = new AccountInterest();
                    }
                    interest_arrs = load_interest(account_interest);
                    
                    if (string.IsNullOrWhiteSpace(selected_interests))
                    {
                        selected_interests = "";
                    }
                    string[] selected_interest_arrs = selected_interests.Split(',');

                #endregion
              
                ViewBag.NotAgree = "";
                ViewBag.Quotas = quotas;
                
            #endregion

            #region custom validates
                #region cookie data
                    if (string.IsNullOrWhiteSpace(phone_number))
                    {
                       return RedirectToAction("RegisterByCust");
                    }
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        return RedirectToAction("RegisterUsername");
                    }
                #endregion

                if (ModelState.ContainsKey("User_Name"))
                    ModelState["User_Name"].Errors.Clear();
                if (ModelState.ContainsKey("Password"))
                    ModelState["Password"].Errors.Clear();

                #region Indentification ID
                    switch (CustomValidate.ValidateIndentification(ac.Identification_Number))
                    {
                        case 0:
                            ModelState.AddModelError("Identification_Number", System.Configuration.ConfigurationManager.AppSettings["Account007"]);
                            break;
                        case 2:
                            ModelState.AddModelError("Identification_Number", System.Configuration.ConfigurationManager.AppSettings["Account007"]);
                            break;
                        case 3:
                            ModelState.AddModelError("Identification_Number", System.Configuration.ConfigurationManager.AppSettings["Account008"]);
                            break;
                        default:
                            break;
                    }
                #endregion

                #region firstname lastname
                    if (string.IsNullOrWhiteSpace(Request.Form["First_Name"]))
                    {
                        ModelState.AddModelError("First_Name", System.Configuration.ConfigurationManager.AppSettings["Account003"]);
                    }

                    if (string.IsNullOrWhiteSpace(Request.Form["Last_Name"]))
                    {
                        ModelState.AddModelError("Last_Name", System.Configuration.ConfigurationManager.AppSettings["Account004"]);
                    }
                #endregion

                #region dob
                    if (string.IsNullOrWhiteSpace(Request.Form["Day_Of_Birth"]) || string.IsNullOrWhiteSpace(Request.Form["Month_Of_Birth"]) || string.IsNullOrWhiteSpace(Request.Form["Year_Of_Birth"]))
                    {
                        ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account020"]);
                    }
                    else
                    {

                        byte day_of_birth = Convert.ToByte(Request.Form["Day_Of_Birth"]);
                        byte month_of_birth = Convert.ToByte(Request.Form["Month_Of_Birth"]);
                        int year_of_birth = Convert.ToInt32(Request.Form["Year_Of_Birth"]);

                        if (month_of_birth == 2)
                        {
                            if (day_of_birth > 29)
                            {
                                ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account019"]);
                            }
                            else
                            {
                                if (!(year_of_birth % 400 == 0 || (year_of_birth % 100 != 0 && year_of_birth % 4 == 0)))
                                {
                                    if (day_of_birth == 29)
                                    {
                                        ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account019"]);
                                    }
                                }

                            }

                        }
                    }
                #endregion

                #region children flag
                    if (!string.IsNullOrEmpty(ac.Children_Flag))
                    {
                        if (ac.Children_Flag.Equals("Y"))
                        {
                            if (ac.Year_Of_Birth_Child1 == null)
                            {
                                ModelState.AddModelError("Year_Of_Birth_Child1", System.Configuration.ConfigurationManager.AppSettings["Account021"]);
                            }
                        }
                    }
                #endregion

                #region Income_Range
                    if (string.IsNullOrWhiteSpace(ac.Income_Range_Cd))
                    {
                        ModelState.AddModelError("Income_Range_Cd", System.Configuration.ConfigurationManager.AppSettings["Account025"]);
                    }
                #endregion

                if (CustomValidate.ValidateZipcode(ac.ZipCode) != 1)
                {
                   ModelState.AddModelError("ZipCode", System.Configuration.ConfigurationManager.AppSettings["Account023"]);
                }

                #region agree flag 
                    if (!agree_flag.Equals("true"))
                    {
                        ViewBag.NotAgree = System.Configuration.ConfigurationManager.AppSettings["Account006"];
                        ModelState.AddModelError("Account_No", System.Configuration.ConfigurationManager.AppSettings["Account006"]);
                    }
                #endregion
            #endregion

            #region save
               if (ModelState.IsValid)
               {
                   Quota select_quota = QuotaCalculation.Calculate(ac, selected_interests);
                   try
                   {
                   #region transaction

                      var transactionOptions = new TransactionOptions();
                      string check_account_status = "";
                      transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                      transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                      var db_transaction = new EchoContext();
                      //SqlTransaction transaction = db_transaction.Database.
                      
                      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions))
                      {

                      
  
                       if (GetCookie("Register", "aid") != null)
                       {
                           int account_id = Convert.ToInt32(GetCookie("Register", "aid"));
                           ac = db_transaction.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
                           check_account_status = ac.Status_Cd.ToString();
                           edited = true;
                           ac.Updated_Dttm = timestamp;
                       }
                       else
                       {
                           ac.Channel_Cd = Channel.web_channel();
                           ac.Created_Dttm = timestamp;
                           ac.Updated_Dttm = timestamp;
                           ac.Registration_Dttm = timestamp;
                           ac.First_Mobile_Number = phone_number;
                       }

                       ac.User_Name = username;
                       ac.Password = password;
                       ac.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                           if (edited)
                           {
                               #region edit
                                   
                                   //db_transaction.Entry(ac).State = EntityState.Modified;
                                   AccountQuota aq = db_transaction.AccountQuotas.SingleOrDefault(x => x.Account_Id == ac.Account_Id);
                                   if (aq != null)
                                   { db_transaction.AccountQuotas.Remove(aq); }
                                   AccountQuota new_aq = new AccountQuota();
                                   new_aq.Account = ac;
                                   new_aq.Quota_Cd = select_quota.Quota_Cd;
                                   db_transaction.AccountQuotas.Add(new_aq);

                                   

                                   if (ac.Status_Cd.ToString().Trim().Equals(FreebieStatus.AccountPTUU()) || ac.Status_Cd.ToString().Trim().Equals(FreebieStatus.AccountPTU()))
                                   {
                                       ac.First_Quota_Cd = select_quota.Quota_Cd;
                                       ac.First_Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                                       ac.First_Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);
                                       ac.Registration_Dttm = timestamp;
                                      
                                       #region account quota used cur
                                           var today = DateTime.Now.Date;
                                           AccountQuotaUsedCur aquc = new AccountQuotaUsedCur();
                                           aquc.Date = today.Date;
                                           aquc.Account = ac;
                                           aquc.Quota_Freq_Used_Val = 0;
                                           aquc.Quota_Avail_Flag = true;
                                           aquc.Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);
                                           aquc.Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                                           db_transaction.AccountQuotaUsedCurs.Add(aquc);
                                       #endregion

                                       SqlParameter output = new SqlParameter("acstatus", SqlDbType.Int);
                                       output.Direction = ParameterDirection.Output;

                                       SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                                       date.Value = DateTime.Now;

                                       SqlParameter no_acct_total = new SqlParameter("no_acct_limit_total", SqlDbType.Int);
                                       SqlParameter system_username = new SqlParameter("created_by_system_name", SqlDbType.VarChar);

                                       int no_acct_limit_total = 0;
                                       system_username.Value = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];

                                       AdminConfiguration admin_config = db_transaction.AdminConfigurations.SingleOrDefault();

                                       if (admin_config != null)
                                       {
                                           no_acct_limit_total = admin_config.No_Activation_Limit_Total;
                                       }

                                       no_acct_total.Value = no_acct_limit_total;

                                       string sql_string =
                                            "declare @tbt table (acstatus int)" +
                                            "UPDATE Account_Activation " +
                                            "SET No_Activation = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation ELSE No_Activation + 1 END, " +
                                            "No_Activation_Acc = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation_Acc ELSE No_Activation_Acc + 1 END, " +
                                            "No_Activation_Pending = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation_Pending + 1 ELSE No_Activation_Pending END, " +
                                            "Updated_By = 'WEBSITE', " +
                                            "Updated_Dttm = GETDATE() " +
                                            "OUTPUT CASE WHEN (deleted.No_Activation + 1 > deleted.No_Max_Activation or deleted.No_Activation_Acc + 1 > @no_acct_limit_total) THEN 1 ELSE 0 END " +
                                            " into @tbt " +
                                            "WHERE [Date] = @today " +
                                            "select @acstatus = acstatus from @tbt";

                                       db_transaction.Database.ExecuteSqlCommand(sql_string, no_acct_total, date, output);

                                       int result = Convert.ToInt16(output.Value);

                                       if (result == 0)
                                       {
                                           ac.Status_Cd = FreebieStatus.AccountActivated();
                                           ac.Activation_Dttm = timestamp;
                                       }
                                       else
                                       { ac.Status_Cd = FreebieStatus.AccountPending(); }
                                       ac.Dummy_Flag = "0";
                                       ac.Channel_Cd = "WEB";
                                   }

                                   
                                   UpdateModel(ac);

                                   AccountInterest aci = db_transaction.AccountInterests.Where(x => x.Account_Id == ac.Account_Id).SingleOrDefault();

                                   bool first_create_aci = false;

                                   if (aci == null)
                                   {
                                       aci = new AccountInterest();
                                       aci.Account = ac;
                                       first_create_aci = true;
                                   }

                                   aci.I01_Food_Dining = selected_interest_arrs.Contains("I01");
                                   aci.I02_Night_Life = selected_interest_arrs.Contains("I02");
                                   aci.I03_Entertainment = selected_interest_arrs.Contains("I03");
                                   aci.I04_Music_Movie = selected_interest_arrs.Contains("I04");
                                   aci.I05_Sports_Fitness = selected_interest_arrs.Contains("I05");
                                   aci.I06_Shopping_Fashion = selected_interest_arrs.Contains("I06");
                                   aci.I07_Health_Beauty = selected_interest_arrs.Contains("I07");
                                   aci.I08_Travel = selected_interest_arrs.Contains("I08");
                                   aci.I09_Pets = selected_interest_arrs.Contains("I09");
                                   aci.I10_Kids_Children = selected_interest_arrs.Contains("I10");
                                   aci.I11_Home_Living = selected_interest_arrs.Contains("I11");
                                   aci.I12_Finance_Investment = selected_interest_arrs.Contains("I12");
                                   aci.I13_Technology_Gadget = selected_interest_arrs.Contains("I13");
                                   aci.I14_Auto = selected_interest_arrs.Contains("I14");
                                   if (first_create_aci)
                                   {
                                       db_transaction.AccountInterests.Add(aci);
                                   }
                                   else
                                   {
                                       UpdateModel(aci);
                                   }
                                   
                                   db_transaction.SaveChanges();
                               #endregion
                           }
                           else
                           {
                               #region create
                                   #region account quota
                                       AccountQuota aq = new AccountQuota();
                                       aq.Account = ac;
                                       aq.Quota_Cd = select_quota.Quota_Cd;
                                       db_transaction.AccountQuotas.Add(aq);
                                   #endregion

                                   #region account mobile
                                       AccountMobile am = new AccountMobile();
                                       am.Mobile_Number = ac.First_Mobile_Number;
                                       am.Account = ac;
                                       am.Status_Cd = FreebieStatus.MobileActive();
                                       am.Primary_Flag = true;
                                       db_transaction.AccountMobiles.Add(am);
                                   #endregion

                                   #region account interests
                                       AccountInterest aci = new AccountInterest();
                                       aci.Account = ac;
                                       aci.I01_Food_Dining = selected_interest_arrs.Contains("I01");
                                       aci.I02_Night_Life = selected_interest_arrs.Contains("I02");
                                       aci.I03_Entertainment = selected_interest_arrs.Contains("I03");
                                       aci.I04_Music_Movie = selected_interest_arrs.Contains("I04");
                                       aci.I05_Sports_Fitness = selected_interest_arrs.Contains("I05");
                                       aci.I06_Shopping_Fashion = selected_interest_arrs.Contains("I06");
                                       aci.I07_Health_Beauty = selected_interest_arrs.Contains("I07");
                                       aci.I08_Travel = selected_interest_arrs.Contains("I08");
                                       aci.I09_Pets = selected_interest_arrs.Contains("I09");
                                       aci.I10_Kids_Children = selected_interest_arrs.Contains("I10");
                                       aci.I11_Home_Living = selected_interest_arrs.Contains("I11");
                                       aci.I12_Finance_Investment = selected_interest_arrs.Contains("I12");
                                       aci.I13_Technology_Gadget = selected_interest_arrs.Contains("I13");
                                       aci.I14_Auto = selected_interest_arrs.Contains("I14");
                                       db_transaction.AccountInterests.Add(aci);
                                   #endregion

                                   #region account quota used cur
                                       var today = DateTime.Now.Date;
                                       AccountQuotaUsedCur aquc = new AccountQuotaUsedCur();
                                       aquc.Date = today.Date;
                                       aquc.Account = ac;
                                       aquc.Quota_Freq_Used_Val = 0;
                                       aquc.Quota_Avail_Flag = true;
                                       aquc.Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);
                                       aquc.Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                                       db_transaction.AccountQuotaUsedCurs.Add(aquc);
                                   #endregion

                                   #region update account activation and set status_cd
                                       SqlParameter output = new SqlParameter("acstatus", SqlDbType.Int);
                                       output.Direction = ParameterDirection.Output;

                                       SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                                       date.Value = DateTime.Now;

                                       SqlParameter no_acct_total = new SqlParameter("no_acct_limit_total", SqlDbType.Int);
                                       SqlParameter system_username = new SqlParameter("created_by_system_name", SqlDbType.VarChar);
                                       
                                       int no_acct_limit_total = 0;
                                       system_username.Value = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];

                                       AdminConfiguration admin_config = db_transaction.AdminConfigurations.SingleOrDefault();

                                       if (admin_config != null)
                                       {
                                           no_acct_limit_total = admin_config.No_Activation_Limit_Total;              
                                       }

                                       no_acct_total.Value = no_acct_limit_total;

                                       string sql_string =
                                            "declare @tbt table (acstatus int)" +
                                            "UPDATE Account_Activation " +
                                            "SET No_Activation = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation ELSE No_Activation + 1 END, " +
                                            "No_Activation_Acc = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation_Acc ELSE No_Activation_Acc + 1 END, " +
                                            "No_Activation_Pending = CASE WHEN (No_Activation + 1 > No_Max_Activation or (No_Activation_Acc + 1 > @no_acct_limit_total)) THEN No_Activation_Pending + 1 ELSE No_Activation_Pending END, " +
                                            "Updated_By = 'WEBSITE', " + 
                                            "Updated_Dttm = GETDATE() " +
                                            "OUTPUT CASE WHEN (deleted.No_Activation + 1 > deleted.No_Max_Activation or deleted.No_Activation_Acc + 1 > @no_acct_limit_total) THEN 1 ELSE 0 END " +
                                            " into @tbt " +
                                            "WHERE [Date] = @today " +
                                            "select @acstatus = acstatus from @tbt";

                                       db_transaction.Database.ExecuteSqlCommand(sql_string, no_acct_total, date, output);

                                       int result = Convert.ToInt16(output.Value);

                                       if (result == 0)
                                       { 
                                           ac.Status_Cd = FreebieStatus.AccountActivated();
                                           ac.Activation_Dttm = timestamp;
                                       }
                                       else
                                       { ac.Status_Cd = FreebieStatus.AccountPending(); }
                                   #endregion

                                   ac.Dummy_Flag = "0";
                                   ac.First_Quota_Cd = select_quota.Quota_Cd;
                                   ac.First_Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                                   ac.First_Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);

                                   db_transaction.Accounts.Add(ac);
                      
                                   db_transaction.SaveChanges();                                
                               #endregion
                           }

                           #region remove otp record and set return message type
                               var otp_date = Convert.ToDateTime(DateTime.Now).Date;
                               OTPRequest otp_request = db_transaction.OTPRequests.Where(x => x.Date.Equals(otp_date)).Where(x => x.PhoneNumber.Equals(ac.First_Mobile_Number)).SingleOrDefault();
                               if (otp_request != null)
                               {
                                   db_transaction.OTPRequests.Remove(otp_request);
                               }

                               if (ac.Status_Cd == FreebieStatus.AccountActivated() || ac.Status_Cd == FreebieStatus.AccountActive() || ac.Status_Cd == FreebieStatus.AccountInActive() || ac.Status_Cd == FreebieStatus.AccountHardSuspend())
                               {
                                   ViewBag.Type = 1;
                               }
                               else
                               {
                                   if (ac.Status_Cd == FreebieStatus.AccountPending())
                                   {
                                       ViewBag.Type = 2;
                                   }
                               }
                               db_transaction.SaveChanges();
                           #endregion

                           ViewBag.Quota_Dur_Val = select_quota.Quota_Dur_Val;
                           ViewBag.Quota_Freq_Val = select_quota.Quota_Freq_Val;
                           scope.Complete();
                           //db_transaction.Dispose();
                       } // end scope

                    #endregion

                       #region call_sp
                      if (!edited || (edited && (check_account_status.Trim().Equals(FreebieStatus.AccountPTUU()) || check_account_status.Trim().Equals(FreebieStatus.AccountPTU()))))
                          {
                              result_sp = CallSP.SP_Insert_Interact_Profile(ac.Account_Id);
                              if (!result_sp[0].Equals("0"))
                              {
                                  using (var db = new EchoContext())
                                  {
                                      SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                                      date.Value = DateTime.Now;

                                      Account remove_ac = db.Accounts.SingleOrDefault(x => x.Account_Id == ac.Account_Id);
                                      if (remove_ac != null)
                                      {
                                          if (remove_ac.Status_Cd.Equals(FreebieStatus.AccountActivated()))
                                          {
                                              string sql_string =
                                                  "UPDATE Account_Activation " +
                                                  "SET No_Activation = CASE WHEN (No_Activation - 1 < 0 ) THEN 0 ELSE No_Activation - 1 END, " +
                                                  "No_Activation_Acc = CASE WHEN (No_Activation_Acc - 1 < 0 ) THEN 0 ELSE No_Activation_Acc - 1 END, " +
                                                  "Updated_By = 'WEBSITE', " +
                                                  "Updated_Dttm = GETDATE() " +
                                                  "WHERE [Date] = @today ";

                                              db.Database.ExecuteSqlCommand(sql_string, date);
                                          }
                                          else
                                          {
                                              if (remove_ac.Status_Cd.Equals(FreebieStatus.AccountPending()))
                                              {
                                                  string sql_string =
                                                     "UPDATE Account_Activation " +
                                                     "SET No_Activation_Pending = CASE WHEN (No_Activation_Pending - 1 < 0 ) THEN 0 ELSE No_Activation_Pending - 1 END, " +
                                                     "Updated_By = 'WEBSITE', " +
                                                     "Updated_Dttm = GETDATE() " +
                                                     "WHERE [Date] = @today ";

                                                  db.Database.ExecuteSqlCommand(sql_string, date);
                                              }
                                          }

                                          AccountQuotaUsedCur remove_aquc = db.AccountQuotaUsedCurs.SingleOrDefault(x => x.Account_Id == ac.Account_Id);
                                          if (remove_aquc != null)
                                          {
                                              db.AccountQuotaUsedCurs.Remove(remove_aquc);
                                          }
                                          db.Accounts.Remove(remove_ac);
                                          db.SaveChanges();
                                      }

                                  }
                                  return_type = 0;
                                  string err_msg = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                                  ModelState.AddModelError("", err_msg);
                                  FreebieEvent.AddCustomError(result_sp[1], Permission.f_cust_regis_page_id);
                              }
                              else
                              {
                                  return_type = 1;
                                  FreebieEvent.AccountCreateEvent(ac, ac.First_Mobile_Number, Permission.f_cust_regis_page_id);
                              }
                          }
                          else
                          {
                              return_type = 1;
                          }
                       #endregion
                   } //end try
                   
                   catch (Exception err)
                   {
                       string err_msg = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                       ModelState.AddModelError("", err_msg);
                    
                       FreebieEvent.AddError(err, Permission.f_cust_regis_page_id);
                   }
                   
               }
            #endregion

               if (return_type == 1)
               {
                   //render success
                   RemoveCoookie("Register");
                   return View("RenderResult");
               }
               else
               {
                   //render fail
                   init_dropdown(ac);
                   ViewBag.Step = 3;
                   interest_arrs = selected_interest_arrs.ToList();
                   ViewBag.InterestSelected = interest_arrs;
                   return View("RegisterProfile", ac); 
               }
        }

        
		public ActionResult ForgotPassword()
		{
            ViewBag.Step = 1;
			return View();
		}

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            EchoContext db = new EchoContext();
            int result = CustomValidate.ValidateEmail(email);
            var client = new SmtpClientWrapper();
            client.SendCompleted += (sender, e) =>
            {
                if (e.Error != null || e.Cancelled)
                {
                    FreebieEvent.AddCustomError(e.Error.Message, Permission.f_update_password_page_id);
                }
                else { 
                
                }
            };
            switch (result)
            { 
                case 0:
                    ModelState.AddModelError("Email", System.Configuration.ConfigurationManager.AppSettings["Account016"]);
                    break;
                case 1:
                    IUserMailer mailer = new UserMailer();
                    Account ac = db.Accounts.SingleOrDefault(x => x.User_Name.Equals(email));
                    string username = "";
                    if (ac != null)
                    {
                        username += ac.First_Name;
                        username += " ";
                        username += ac.Last_Name;
                    }
                    string enc = FreebieCrypt.EncryptStringAES(email, System.Configuration.ConfigurationManager.AppSettings["TEMP_KEY"]);
                    mailer.EnterNewPassword(email, username, enc).SendAsync("log email event", client);

                    EmailRequest er = db.EmailRequests.SingleOrDefault(x => x.Email.Equals(email));
                    int exp_add = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["LINK_EXPIRED"]);

                    if (er == null)
                    {
                        er = new EmailRequest();
                        er.Email = email;
                        er.Expired_Dttm = DateTime.Now.AddMinutes(exp_add);
                        db.EmailRequests.Add(er);
                        db.SaveChanges();
                    }
                    else
                    {
                        er.Expired_Dttm = DateTime.Now.AddMinutes(exp_add);
                        UpdateModel(er);
                        db.SaveChanges();
                    }

                    ViewBag.Type = 3;
                    return View("RenderResult");
                case 2:
                    ModelState.AddModelError("Email", System.Configuration.ConfigurationManager.AppSettings["Account016"]);
                    break;

                case 3:
                    ModelState.AddModelError("Email", System.Configuration.ConfigurationManager.AppSettings["Account018"]);
                    break;
                default:
                    break;

            }
            ViewBag.Step = 1;
            return View();
        }

        public ActionResult EnterNewPassword(string Ref)
        {
            EchoContext db = new EchoContext();
            string email = FreebieCrypt.DecryptStringAES(Ref, System.Configuration.ConfigurationManager.AppSettings["TEMP_KEY"]);
            EmailRequest er = db.EmailRequests.SingleOrDefault(x => x.Email.Equals(email));

            if (er == null)
            {
                return HttpNotFound();
            }
            else
            {
                DateTime req_link = DateTime.Now;
                if (req_link > er.Expired_Dttm)
                {
                    return HttpNotFound(); //link expired
                }            
            }
            
            Account account = db.Accounts.SingleOrDefault(x => x.User_Name.Equals(email));

            if (account == null)
            {
                return HttpNotFound();
            }
            ViewBag.Step = 2;
            ViewBag.Ref = Ref;
            return View(new ChangePassword());
        }

        [HttpPost]
        public ActionResult EnterNewPassword(string Ref, ChangePassword cpwd)
        {
            EchoContext db = new EchoContext();
            string email = FreebieCrypt.DecryptStringAES(Ref, System.Configuration.ConfigurationManager.AppSettings["TEMP_KEY"]);
            Account account = db.Accounts.SingleOrDefault(x => x.User_Name.Equals(email));
            bool flag = true;
            if (account == null)
            {
                return HttpNotFound();
            }

            string new_pwd = Request.Form["New_Password"];
            string cfm_pwd = Request.Form["Confirm_New_Password"];

            if (string.IsNullOrEmpty(new_pwd))
            {
                flag = false;
                ModelState.AddModelError("new_password", System.Configuration.ConfigurationManager.AppSettings["Validate005"]);
            }

            if (string.IsNullOrEmpty(cfm_pwd))
            {
                flag = false;
                ModelState.AddModelError("confirm_new_password", System.Configuration.ConfigurationManager.AppSettings["Validate009"]);
            }

            if (new_pwd.Length < 6 || new_pwd.Length > 15)
            {
                flag = false;
                ModelState.AddModelError("new_password", System.Configuration.ConfigurationManager.AppSettings["Validate008"]);
            }

            if (!new_pwd.Equals(cfm_pwd))
            {
                flag = false;
                ModelState.AddModelError("confirm_new_password", System.Configuration.ConfigurationManager.AppSettings["Validate006"]);
            }

            if (flag)
            {
                string enc = FormsAuthentication.HashPasswordForStoringInConfigFile(new_pwd, "SHA1");
                account.Password = enc;
                account.Updated_By = account.Account_No;
                account.Updated_Dttm = DateTime.Now;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("EnterNewPwdResult");
            }

            ViewBag.Step = 2;
            ViewBag.Ref = Ref;
            return View(new ChangePassword());
        }

        public ActionResult EnterNewPwdResult()
        {

            return View();
        }
		public ActionResult ForgotUsername()
		{
            ViewBag.Step = 1;
            RemoveCoookie("Stats");
            ViewBag.ValidNumber = false;
			return View();
		}

        [HttpPost]
        public ActionResult ForgotUsername(string PhoneNumber, string Password)
        {

            EchoContext db = new EchoContext();
            string phoneNumber = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            int result = CustomValidate.ValidateNumber(phoneNumber);
            ViewBag.ValidNumber = false;
            ViewBag.Step = 1;
            if (result == 0)
            {
                ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account010"]);
                return View();
            }

            AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phoneNumber)).Where(x => x.Status_Cd.Equals("AC")).SingleOrDefault();

            if (am == null)
            {
                ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account011"]);
                return View();
            }

            Account ac = am.Account;

            if (string.IsNullOrWhiteSpace(ac.User_Name) || (string.IsNullOrWhiteSpace(ac.Password)))
            {
                ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account022"]);
                return View();
            }

            string pwd_enc = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");

            if (!ac.Password.Equals(pwd_enc))
            {
                ModelState.AddModelError("Password", System.Configuration.ConfigurationManager.AppSettings["Account012"]);
                return View();
            }

            //send otp
            ViewBag.ValidNumber = true;
            string otp = OTPHandler.SendOTPUsername(phoneNumber);
            if (otp.Equals("limit_daily"))
            {
                string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp01"];
                err_str = err_str.Replace("{count}", System.Configuration.ConfigurationManager.AppSettings["OTP_ALLOW_PER_DAY_PER_NUMBER"]);
                ViewBag.ErrorOTP = err_str;
            }
            else
            {
                if (otp.Equals("limit_interval"))
                {
                    string err_str = System.Configuration.ConfigurationManager.AppSettings["Otp02"];
                    err_str = err_str.Replace("{minutes}", System.Configuration.ConfigurationManager.AppSettings["INTERVAL_PERIOD_BETWEEN_OTP"]);
                    ViewBag.ErrorOTP = err_str;
                }
            }
            ViewBag.PhoneNumber = phoneNumber;
            AddCookie("Stats", new string[] { "pn" }, new string[] { phoneNumber });
            ViewBag.OTP = otp;                  
            ViewBag.Step = 2;
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword()
        {
            ViewBag.Type = 4;
            return View("RenderResult");
        }

        [HttpPost]
        public ActionResult GetUsername()
        {
            string phoneNumber = GetCookie("Stats", "pn");
            EchoContext db = new EchoContext();
            bool flag = true;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return RedirectToAction("ForgotUsername");
            }

            ViewBag.PhoneNumber = phoneNumber;

            string otp = Request.Form["OTPPassword"];

            if (flag && (string.IsNullOrEmpty(otp) || otp.Length < 4))
            {
                ViewBag.ValidNumber = true;
                ViewBag.Error = true;
                ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate010"];
                flag = false;
            }

            if (flag)
            {
                int result = OTPHandler.ValidateOTP(phoneNumber, otp);
                switch (result)
                {
                    case 0:
                        // send username
                        Message.SendUsername(phoneNumber);

                        ViewBag.Type = 5;
                        return View("RenderResult");
                    case 1:
                        ViewBag.Error = true;
                        ViewBag.ValidNumber = true;
                        ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate007"];
                        break;
                    case 2:
                        ViewBag.ValidNumber = false;
                        ViewBag.PhoneNumber = "";
                        ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp03"];
                        RemoveCoookie("Stats");
                        break;
                    case 3:
                        ViewBag.ValidNumber = false;
                        ViewBag.PhoneNumber = "";
                        ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp04"];
                        RemoveCoookie("Stats");
                        break;
                    default:
                        break;
                }
            }
            ViewBag.Step = 2;
            return View("ForgotUsername");
        }

		public int ValidateEmail(string email)
		{
			var valid = false;
            var status = 3;
            EchoContext db = new EchoContext();
			if (String.IsNullOrEmpty(email))
                return 0;


            string strPattern = CustomValidate.EmailRegEx();


			if (System.Text.RegularExpressions.Regex.IsMatch(email, strPattern))
			{ valid = true; }

			var check_email = db.Accounts.SingleOrDefault(a => a.User_Name == email);

            if (!valid) { status = 1; }
            if (check_email != null) { status = 2; }
            return status; //(check_email == null) && valid;
		}

		public void init_dropdown(Account account)
		{
            EchoContext db = new EchoContext();
            var blank_item = new SelectListItem()
            {
                Text = "-",
                Value = "",
                Selected = false
            };
            years.Add(blank_item);
            months.Add(blank_item);
            days.Add(blank_item);
            child1_years.Add(blank_item);
            child2_years.Add(blank_item);
            child3_years.Add(blank_item);
            for (int i = 0; i <= 19; i++)
            {
                var text = "";
                if (i == 0)
                {
                    text = "<1";
                }
                else
                {
                    if (i > 18)
                    {
                        text = "18+";
                    }
                    else
                    {
                        text = i.ToString();
                    }
                }
                var cy1 = new SelectListItem()
                {
                    Text = text,
                    Value = (current_year - i).ToString(),
                    Selected = (account.Year_Of_Birth_Child1 == (current_year - i))
                };
                var cy2 = new SelectListItem()
                {
                    Text = text,
                    Value = (current_year - i).ToString(),
                    Selected = (account.Year_Of_Birth_Child2 == (current_year - i))
                };
                var cy3 = new SelectListItem()
                {
                    Text = text,
                    Value = (current_year - i).ToString(),
                    Selected = (account.Year_Of_Birth_Child3 == (current_year - i))
                };
                child1_years.Add(cy1);
                child2_years.Add(cy2);
                child3_years.Add(cy3);
            }
			for (int i = current_year; i >= (current_year - 120); i--)
			{
				var y = new SelectListItem()
				{
                    Text = (i + 543).ToString(),
					Value = i.ToString(),
                    Selected = (account.Year_Of_Birth == i)
				};
				years.Add(y);
			}

			for (int i = 1; i <= 31; i++)
			{
				var d = new SelectListItem()
				{
					Text = i.ToString(),
					Value = i.ToString(),
                    Selected = (account.Day_Of_Birth == i)
				};
				days.Add(d);
			}

            string[] month_arrs = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };
			for (int i = 1; i <= 12; i++)
			{
				var m = new SelectListItem()
				{
					Text = month_arrs[i-1],
					Value = i.ToString(),
                    Selected = (account.Month_Of_Birth == i)
				};
				months.Add(m);
			}
			ViewBag.Day_Of_Birth = days;
			ViewBag.Month_Of_Birth = months;
			ViewBag.Year_Of_Birth = years;

            List<SelectListItem> marital_status_items = new List<SelectListItem>();
            marital_status_items.Add(blank_item);
            IEnumerable<MaritalStatus> marital_statuses = db.MaritalStatuses.OrderBy(s => s.Marital_Status_Name_Th);
            foreach (var ms in marital_statuses)
            {
                var item = new SelectListItem()
                {
                    Text = ms.Marital_Status_Name_Th,
                    Value = ms.Marital_Status_Cd,
                    Selected = ms.Marital_Status_Cd.Equals(account.Marital_Status_Cd)
                };
                marital_status_items.Add(item);
            }

            ViewBag.Marital_Status_Cd = marital_status_items;

            List<SelectListItem> income_range_items = new List<SelectListItem>();
            income_range_items.Add(blank_item);
            IEnumerable<IncomeRange> income_ranges = db.IncomeRanges.OrderBy(x => x.Income_Range_Cd);
            foreach (var ir in income_ranges)
            {
                var item = new SelectListItem()
                {
                    Text = ir.Income_Range_Desc_Th,
                    Value = ir.Income_Range_Cd,
                    Selected = ir.Income_Range_Cd.Equals(account.Income_Range_Cd)
                };
                income_range_items.Add(item);
            }
            List<SelectListItem> personal_income_range_items = new List<SelectListItem>();
            personal_income_range_items.Add(blank_item);
            IEnumerable<PersonalIncomeRange> personal_income_ranges = db.PersonalIncomeRanges.OrderBy(x => x.Personal_Income_Range_Cd);
            foreach (var pir in personal_income_ranges)
            {
                var item = new SelectListItem()
                {
                    Text = pir.Personal_Income_Range_Desc_Th,
                    Value = pir.Personal_Income_Range_Cd,
                    Selected = pir.Personal_Income_Range_Cd.Equals(account.Personal_Income_Range_Cd)
                };
                personal_income_range_items.Add(item);
            }

            List<SelectListItem> occupation_items = new List<SelectListItem>();
            occupation_items.Add(blank_item);
            IEnumerable<Occupation> occupations = db.Occupations.OrderBy(x => x.Occupation_Cd);
            foreach (var o in occupations)
            {
                var item = new SelectListItem()
                {
                    Text = o.Occupation_Name_Th,
                    Value = o.Occupation_Cd,
                    Selected = o.Occupation_Cd.Equals(account.Occupation_Cd)
                };
                occupation_items.Add(item);
            }
            
            List<SelectListItem> education_items = new List<SelectListItem>();
            education_items.Add(blank_item);
            IEnumerable<Education> educations = db.Educations.OrderBy(x => x.Education_Cd);
            foreach (var ed in educations)
            {
                var item = new SelectListItem()
                {
                    Text = ed.Education_Name_Th,
                    Value = ed.Education_Cd,
                    Selected = ed.Education_Cd.Equals(account.Education_Cd)
                };
                education_items.Add(item);
            }

            List<SelectListItem> areacode_items = new List<SelectListItem>();
            areacode_items.Add(blank_item);
            IEnumerable<Zipcode> zipcodes = db.Zipcodes.OrderBy(x => x.ZipCode);
            foreach (var zc in zipcodes)
            {
                var item = new SelectListItem()
                {
                    Text = zc.District,
                    Value = zc.AreaCode,
                    Selected = zc.AreaCode.Equals(account.AreaCode)
                };
                areacode_items.Add(item);
            }

            ViewBag.Income_Range_Cd = income_range_items;
            ViewBag.Personal_Income_Range_Cd = personal_income_range_items;
            ViewBag.Occupation_Cd = occupation_items;
            ViewBag.Education_Cd = education_items;
            ViewBag.AreaCode = areacode_items;
            ViewBag.Year_Of_Birth_Child1 = child1_years;
            ViewBag.Year_Of_Birth_Child2 = child2_years;
            ViewBag.Year_Of_Birth_Child3 = child3_years;
			ViewBag.Interests = db.Interests.ToList();
		}

        public ActionResult RegisterResultSuccess()
        {
            return View();
        }
        public ActionResult RegisterResultPending()
        {
            return View();
        }
        public ActionResult EULA()
        {
            ViewBag.Type = 1;
            return View("RenderDialog");
        }
        public ActionResult IndividualPolicy()
        {
            ViewBag.Type = 2;
            return View("RenderDialog");
        }
        private void AddCookie(string ckname, string[] key, string[] value)
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

        private string GetCookie(string ckname, string key)
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

        private void RemoveCoookie(string ckname)
        {
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays( -1 );
                Response.Cookies.Add(cookie);
            }
        }

        [HttpGet]
        public ActionResult FindAreacode(string zipcode, string areacode)
        {
           JsonResult json = null;
           var db = new EchoContext();
           if (string.IsNullOrWhiteSpace(zipcode))
           {
               zipcode = "";
           }
           if (string.IsNullOrWhiteSpace(areacode))
           {
               areacode = "";
           }
           IEnumerable<ZipcodeAreacode> zipcodes;
           zipcodes = from z in db.Zipcodes
                      where (z.ZipCode.Contains(zipcode)) && (z.District.Contains(areacode))
                      select new ZipcodeAreacode
                      {
                         ZipCode = z.ZipCode,
                         AreaCode = z.AreaCode,
                         District = z.District
                      };
           json = Json(zipcodes, JsonRequestBehavior.AllowGet);
           return json;
        }
       

        public List<string> load_interest(AccountInterest aci)
        {
            List<string> arrs = new List<string>();

            if (aci.I01_Food_Dining) { arrs.Add("I01"); }
            if (aci.I02_Night_Life) { arrs.Add("I02"); }
            if (aci.I03_Entertainment) { arrs.Add("I03"); }
            if (aci.I04_Music_Movie) { arrs.Add("I04"); }
            if (aci.I05_Sports_Fitness) { arrs.Add("I05"); }
            if (aci.I06_Shopping_Fashion) { arrs.Add("I06"); }
            if (aci.I07_Health_Beauty) { arrs.Add("I07"); }
            if (aci.I08_Travel) { arrs.Add("I08"); }
            if (aci.I09_Pets) { arrs.Add("I09"); }
            if (aci.I10_Kids_Children) { arrs.Add("I10"); }
            if (aci.I11_Home_Living) { arrs.Add("I11"); }
            if (aci.I12_Finance_Investment) { arrs.Add("I12"); }
            if (aci.I13_Technology_Gadget) { arrs.Add("I13"); }
            if (aci.I14_Auto) { arrs.Add("I14"); }

            return arrs;
        }

    }
}
