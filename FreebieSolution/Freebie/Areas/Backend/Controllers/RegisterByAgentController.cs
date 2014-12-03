using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Libs;
using System.Collections;
using Freebie.Models;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace Freebie.Areas.Backend.Controllers
{
    public class RegisterByAgentController : Controller
    {
        //
        // GET: /Backend/RegisterByAgent/
        private EchoContext db = new EchoContext();
        int current_year = DateTime.Now.Year;
        List<SelectListItem> years = new List<SelectListItem>();
        List<SelectListItem> months = new List<SelectListItem>();
        List<SelectListItem> days = new List<SelectListItem>();
        List<SelectListItem> child1_years = new List<SelectListItem>();
        List<SelectListItem> child2_years = new List<SelectListItem>();
        List<SelectListItem> child3_years = new List<SelectListItem>();

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult Index()
        {
            ViewBag.Step = 1;
            ViewBag.ValidNumber = false;
            RemoveCoookie("Register");
            return View();
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult VerifySubrNumber()
        {
            string phoneNumber = Request.Form["PhoneNumber"];
            ViewBag.Step = 1;
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.Path = "Index";
            int result = CustomValidate.ValidateNumber(phoneNumber);
            ViewBag.ValidNumber = false;
            switch (result)
            {
                case 0:
                    ViewBag.Type = 1;
                    ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account010"]);
                    return View("Index");
                case 1:
                    
                    AddCookie("Register", new string[] { "phone_number" }, new string[] { phoneNumber });
                    AddCookie("Register", new string[] { "otp_pass" }, new string[] { "yes" });
                    return RedirectToAction("RegisterProfileByAgent");
                    //return View("Index");
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
                    ViewBag.Type = 2;
                    return View("RenderStatics");
                case 6:
                    ViewBag.Type = 3;
                    return View("RenderStatics");
                case 7:
                    //AddCookie("Register", new string[] { "phone_number" }, new string[] { phoneNumber });
                    //AddCookie("Register", new string[] { "otp_pass" }, new string[] { "yes" });
                    string inactive = FreebieStatus.MobileInActive();
                    AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phoneNumber)).Where(x => !x.Status_Cd.Equals(inactive)).SingleOrDefault();
                    Account ac = db.Accounts.SingleOrDefault(x => x.Account_Id == am.Account_Id);
                    if (ac != null)
                    {
                        AddCookie("Register", new string[] { "aid", "phone_number", "otp_pass" }, new string[] { ac.Account_Id.ToString(), phoneNumber, "yes" });
                    }
                    return RedirectToAction("RegisterProfileByAgent");
                default:
                    ViewBag.Type = 1;
                    return View("Index");
            }
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult Register(string phone_number)
        {
            string otp = Request.Form["Password"];
            phone_number = GetCookie("register", "phone_number");
            bool flag = true;
            if (string.IsNullOrEmpty(phone_number))
            {
                ViewBag.ValidNumber = false;
                flag = false;
            }
            ViewBag.PhoneNumber = phone_number;
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
                        AddCookie("Register", new string[] { "otp_pass" }, new string[] { "yes" });
                        return RedirectToAction("RegisterProfileByAgent");
                    case 1:
                        ViewBag.Error = true;
                        ViewBag.ValidNumber = true;
                        ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate007"];
                        break;
                    case 2:
                        ViewBag.ValidNumber = false;
                        ViewBag.PhoneNumber = "";
                        ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp03"];
                        RemoveCoookie("register");
                        break;
                    default:
                        break;
                }
            }
            ViewBag.Step = 1;
            return View("Index");

        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult RegisterProfileByAgent()
        {
            var db = new EchoContext();
            string phone_number = GetCookie("Register", "phone_number");
            string otp_pass = GetCookie("Register", "otp_pass");
            if (string.IsNullOrEmpty(phone_number) || string.IsNullOrEmpty(otp_pass) || (!string.IsNullOrEmpty(otp_pass) && !otp_pass.Equals("yes")))
            {
                return RedirectToAction("Index");
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
            init_dropdown(ac);
            ViewBag.Step = 2;
            ViewBag.InterestSelected = new List<string>();
            return View(ac);
        }

        [HttpPost]
        [SessionExpireFilter]
        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
       
        public ActionResult CreateAccount(Account ac)
        {
            
            
            var db = new EchoContext();
            #region get infomations and variables
                string phone_number = GetCookie("Register", "phone_number");
                int return_type = 0;
                string[] result_sp = new string[2];
                IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
                Hashtable quotas = new Hashtable();
                var selected_interests = Request.Form["selectedInterests"];
                var agree_flag = Request.Form["Agree"];
                

                if (string.IsNullOrWhiteSpace(agree_flag))
                {
                    agree_flag = "";
                }

                #region init data
                    quotas["low"] = new Hashtable();
                    quotas["medium"] = new Hashtable();
                    quotas["high"] = new Hashtable();
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
                        return RedirectToAction("Index");
                    }
                #endregion

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

                if (ModelState.ContainsKey("User_Name"))
                    ModelState["User_Name"].Errors.Clear();
                if (ModelState.ContainsKey("Password"))
                    ModelState["Password"].Errors.Clear();

                if (string.IsNullOrWhiteSpace(Request.Form["First_Name"]))
                {
                    ModelState.AddModelError("First_Name", System.Configuration.ConfigurationManager.AppSettings["Account003"]);
                }

                if (string.IsNullOrWhiteSpace(Request.Form["Last_Name"]))
                {
                    ModelState.AddModelError("Last_Name", System.Configuration.ConfigurationManager.AppSettings["Account004"]);
                }


                if (ac.Day_Of_Birth == null || ac.Month_Of_Birth == null || ac.Year_Of_Birth == null)
                {
                    ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account020"]);
                }

                if (ac.Month_Of_Birth == 2)
                {
                    if (ac.Day_Of_Birth > 29)
                    {
                        ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account019"]);
                    }
                    else
                    {
                        if (!(ac.Year_Of_Birth % 400 == 0 || (ac.Year_Of_Birth % 100 != 0 && ac.Year_Of_Birth % 4 == 0)))
                        {
                            if (ac.Day_Of_Birth == 29)
                            {
                                ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account019"]);
                            }
                        }

                    }

                }
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

                if (CustomValidate.ValidateZipcode(ac.ZipCode) != 1)
                {
                    ModelState.AddModelError("ZipCode", System.Configuration.ConfigurationManager.AppSettings["Account023"]);
                }

                if (!agree_flag.Equals("true"))
                {
                    ViewBag.NotAgree = System.Configuration.ConfigurationManager.AppSettings["Account006"];
                    ModelState.AddModelError("Account_No", System.Configuration.ConfigurationManager.AppSettings["Account006"]);
                }
            #endregion

            #region save
                bool edited = false;
                if (ModelState.IsValid)
                {
                   Quota select_quota = QuotaCalculation.Calculate(ac, selected_interests);
                   try
                   {
                       #region transaction
                           var transactionOptions = new TransactionOptions();
                           transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                           transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                           DateTime timestamp = DateTime.Now;
                           using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
                           {
                              
                               var db_transaction = new EchoContext();
                                string username = User.Identity.Name;
                               User user = db_transaction.Users.SingleOrDefault(x => x.User_Name.Equals(username));

                               if (GetCookie("Register", "aid") != null)
                               {
                                   int account_id = Convert.ToInt32(GetCookie("Register", "aid"));
                                   ac = db_transaction.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
                                   edited = true;
                                   ac.Updated_Dttm = timestamp;
                                   ac.Updated_By = user.User_No;
                               }
                               else
                               {
                                   ac.Channel_Cd = Channel.web_channel();
                                   ac.Created_Dttm = timestamp;
                                   ac.Updated_Dttm = timestamp;
                                   ac.Registration_Dttm = timestamp;
                                   ac.First_Mobile_Number = phone_number;
                               }

                               
                               ac.Staff_No = user.User_No;

                               if (edited)
                               {
                                   #region edit
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
                                           string[] interest_arrs = selected_interest_arrs;
                                           aci.Account = ac;
                                           aci.I01_Food_Dining = interest_arrs.Contains("I01");
                                           aci.I02_Night_Life = interest_arrs.Contains("I02");
                                           aci.I03_Entertainment = interest_arrs.Contains("I03");
                                           aci.I04_Music_Movie = interest_arrs.Contains("I04");
                                           aci.I05_Sports_Fitness = interest_arrs.Contains("I05");
                                           aci.I06_Shopping_Fashion = interest_arrs.Contains("I06");
                                           aci.I07_Health_Beauty = interest_arrs.Contains("I07");
                                           aci.I08_Travel = interest_arrs.Contains("I08");
                                           aci.I09_Pets = interest_arrs.Contains("I09");
                                           aci.I10_Kids_Children = interest_arrs.Contains("I10");
                                           aci.I11_Home_Living = interest_arrs.Contains("I11");
                                           aci.I12_Finance_Investment = interest_arrs.Contains("I12");
                                           aci.I13_Technology_Gadget = interest_arrs.Contains("I13");
                                           aci.I14_Auto = interest_arrs.Contains("I14");
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

                                               int no_acct_limit_total = 0;
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

                                               db.Database.ExecuteSqlCommand(sql_string, no_acct_total, date, output);

                                               int result = Convert.ToInt16(output.Value);

                                               if (result == 0)
                                               {
                                                   ac.Status_Cd = FreebieStatus.AccountActivated();
                                                   ac.Activation_Dttm = timestamp;
                                               }
                                               else
                                               { ac.Status_Cd = FreebieStatus.AccountPending(); }
                                   #endregion
                                   #endregion

                                   ac.Dummy_Flag = "0";
                                   ac.First_Quota_Cd = select_quota.Quota_Cd;
                                   ac.First_Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                                   ac.First_Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);

                                   db_transaction.Accounts.Add(ac);
                                   db_transaction.SaveChanges();
                               }
                               
                             

                              

                               //result_sp = CallSP.SP_Insert_Interact_Profile(ac.Account_Id);
                               ViewBag.Quota_Dur_Val = select_quota.Quota_Dur_Val;
                               ViewBag.Quota_Freq_Val = select_quota.Quota_Freq_Val;

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

                               scope.Complete();
                           }
                       #endregion
                       #region call_sp
                           result_sp = CallSP.SP_Insert_Interact_Profile(ac.Account_Id);
                           if (!result_sp[0].Equals("0"))
                           {
                               using (var new_db = new EchoContext())
                               {
                                   SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                                   date.Value = DateTime.Now;
                                   Account remove_ac = new_db.Accounts.SingleOrDefault(x => x.Account_Id == ac.Account_Id);
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

                                           new_db.Database.ExecuteSqlCommand(sql_string, date);
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

                                               new_db.Database.ExecuteSqlCommand(sql_string, date);
                                           }
                                       }
                                       AccountQuotaUsedCur remove_aquc = new_db.AccountQuotaUsedCurs.SingleOrDefault(x => x.Account_Id == ac.Account_Id);
                                       if (remove_aquc != null)
                                       {
                                           new_db.AccountQuotaUsedCurs.Remove(remove_aquc);
                                       }
                                       new_db.Accounts.Remove(remove_ac);
                                       new_db.SaveChanges();
                                   }
                               }
                               return_type = 0;
                               string err_msg = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                               ModelState.AddModelError("", err_msg);
                               FreebieEvent.AddCustomError(result_sp[1], Permission.register_page_id);
                           }
                           else
                           {
                               return_type = 1;
                               FreebieEvent.AccountCreateEvent(ac, ac.First_Mobile_Number, Permission.register_page_id);
                           }
                       #endregion
                   }
                   catch (Exception err)
                   {
                       string err_msg = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                       ModelState.AddModelError("", err_msg);
                       init_dropdown(ac);
                       FreebieEvent.AddError(err, Permission.register_page_id);
                   }
                }
            #endregion
                               
            if (return_type == 1)
            {
                RemoveCoookie("Register");
                return View("RenderResult");
            }
            else
            {
                // render fail
                init_dropdown(ac);
                ViewBag.Step = 2;
                ViewBag.InterestSelected = selected_interest_arrs.ToList();
                return View("RegisterProfileByAgent", ac);
            }  
        }


        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult after_create_account(int account_id, string selected_interests)
        {
            using (var echo = new EchoContext())
            {
                Account account = echo.Accounts.SingleOrDefault(x => x.Account_Id.Equals(account_id));

                if (account != null)
                {
                    // call store procedure
                    string[] result_sp = new string[2];
                    result_sp = CallSP.SP_Insert_Interact_Profile(account.Account_Id);

                    if (!string.IsNullOrWhiteSpace(result_sp[0]))
                    {
                        int err_id = Convert.ToInt16(result_sp[0]);
                        if (err_id != 0)
                        {
                            // remove account
                            Account temp_account = new Account();
                            temp_account = account;

                            echo.Accounts.Remove(account);
                            echo.SaveChanges();

                            string err_msg = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                            ModelState.AddModelError("", err_msg);
                            init_dropdown(temp_account);
                            FreebieEvent.AddCustomError(result_sp[1], Permission.register_page_id);
                            ViewBag.Step = 3;
                            IEnumerable<Quota> base_quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).OrderBy(x => x.Quota_Cd);
                            Hashtable quotas = new Hashtable();
                            quotas["low"] = new Hashtable();
                            quotas["medium"] = new Hashtable();
                            quotas["high"] = new Hashtable();
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
                            ViewBag.InterestSelected = new List<string>();
                            if (selected_interests != null)
                            {
                                string[] interest_arrs = selected_interests.Split(',');
                                ViewBag.InterestSelected = interest_arrs.ToList();
                            }
                            
                            return View("RegisterProfileByAgent", temp_account);
                        }
                    }
                    echo.Entry(account).State = EntityState.Modified;
                    AccountMobile am = new AccountMobile();
                    am.Mobile_Number = account.First_Mobile_Number;
                    am.Account_Id = account.Account_Id;
                    am.Status_Cd = FreebieStatus.MobileActive();
                    am.Primary_Flag = true;
                    echo.AccountMobiles.Add(am);
                    AccountInterest aci = new AccountInterest();
                    aci.Account_Id = account.Account_Id;
                    if (selected_interests != null)
                    {
                        string[] interest_arrs = selected_interests.Split(',');
                        aci.I01_Food_Dining = interest_arrs.Contains("I01");
                        aci.I02_Night_Life = interest_arrs.Contains("I02");
                        aci.I03_Entertainment = interest_arrs.Contains("I03");
                        aci.I04_Music_Movie = interest_arrs.Contains("I04");
                        aci.I05_Sports_Fitness = interest_arrs.Contains("I05");
                        aci.I06_Shopping_Fashion = interest_arrs.Contains("I06");
                        aci.I07_Health_Beauty = interest_arrs.Contains("I07");
                        aci.I08_Travel = interest_arrs.Contains("I08");
                        aci.I09_Pets = interest_arrs.Contains("I09");
                        aci.I10_Kids_Children = interest_arrs.Contains("I10");
                        aci.I11_Home_Living = interest_arrs.Contains("I11");
                        aci.I12_Finance_Investment = interest_arrs.Contains("I12");
                        aci.I13_Technology_Gadget = interest_arrs.Contains("I13");
                        aci.I14_Auto = interest_arrs.Contains("I14");
                    }
                    else
                    {

                    }
                    echo.AccountInterests.Add(aci);
                    Quota select_quota = QuotaCalculation.Calculate(account, selected_interests);
                    account.First_Quota_Cd = select_quota.Quota_Cd;
                    account.First_Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                    account.First_Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);

                    AccountQuota aq = new AccountQuota();
                    aq.Account_Id = account.Account_Id;
                    aq.Quota_Cd = select_quota.Quota_Cd;

                    echo.AccountQuotas.Add(aq);
                    var today = DateTime.Now.Date;
                    AccountQuotaUsedCur aquc = echo.AccountQuotaUsedCurs.Where(x => x.Date.Equals(today)).Where(x => x.Account_Id == account.Account_Id).SingleOrDefault();

                    if (aquc == null)
                    {
                        aquc = new AccountQuotaUsedCur();
                        aquc.Date = today.Date;
                        aquc.Account_Id = account.Account_Id;
                        aquc.Quota_Freq_Used_Val = 0;
                        aquc.Quota_Avail_Flag = true;
                        aquc.Quota_Dur_Val = Convert.ToByte(select_quota.Quota_Dur_Val);
                        aquc.Quota_Freq_Val = Convert.ToByte(select_quota.Quota_Freq_Val);
                        echo.AccountQuotaUsedCurs.Add(aquc);

                    }

                    //remove otp record
                    var date = Convert.ToDateTime(DateTime.Now).Date;
                    OTPRequest otp_request = echo.OTPRequests.Where(x => x.Date.Equals(date)).Where(x => x.PhoneNumber.Equals(account.First_Mobile_Number)).SingleOrDefault();
                    if (otp_request != null)
                    {
                        echo.OTPRequests.Remove(otp_request);
                    }

                    echo.SaveChanges();
                    if (account.Status_Cd == FreebieStatus.AccountActivated())
                    {
                        ViewBag.Type = 1;
                    }
                    else
                    {
                        if (account.Status_Cd == FreebieStatus.AccountPending())
                        {
                            ViewBag.Type = 2;
                        }
                    }
                    FreebieEvent.AccountCreateEvent(account, account.First_Mobile_Number, Permission.register_page_id);
                    RemoveCoookie("Register");
                    ViewBag.Quota_Dur_Val = select_quota.Quota_Dur_Val;
                    ViewBag.Quota_Freq_Val = select_quota.Quota_Freq_Val;
                    CallSP.SP_Insert_Interact_Profile(account.Account_Id);

                    return View("RenderResult");
                }
                else
                {
                    return HttpNotFound();
                }

            }
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
                //cookie = HttpCookieEncryption.Decrypt(cookie);//ORIGIN
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

            //HttpCookie cookie_enc = HttpCookieEncryption.Encrypt(cookie);//ORIGIN
            //this.ControllerContext.HttpContext.Response.Cookies.Add(cookie_enc);//ORIGIN
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        private string GetCookie(string ckname, string key)
        {
            string value = "";
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie == null)
            {
                return value;
            }
            //cookie = HttpCookieEncryption.Decrypt(cookie);//ORIGIN
            value = cookie[key];
            return value;
        }

        private void RemoveCoookie(string ckname)
        {
            HttpCookie cookie = HttpContext.Request.Cookies.Get(ckname);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }
        public void init_dropdown(Account account)
        {
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
            var db = new EchoContext();

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
    }
}
