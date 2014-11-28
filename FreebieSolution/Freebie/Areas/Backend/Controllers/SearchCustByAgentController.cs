using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Models;
using Freebie.ViewModels;
using PagedList;
using Freebie.Libs;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Collections;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Web.Security;
using System.Data.SqlClient;
namespace Freebie.Areas.Backend.Controllers
{
    public class SearchCustByAgentController : Controller
    {
        //
        // GET: /Backend/SearchCustByAgent/
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
            return View();
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult SearchCustResult(int? page, string phone_number, string idcard, string email)
        {
            if (!string.IsNullOrWhiteSpace(Request.Form["PhoneNumber"]))
            {
                phone_number = Request.Form["PhoneNumber"];
            }
            if (!string.IsNullOrWhiteSpace(Request.Form["Identification_Number"]))
            {
                idcard = Request.Form["Identification_Number"];
            }
            if (!string.IsNullOrWhiteSpace(Request.Form["Email"]))
            {
                email = Request.Form["Email"];
            }
            

            if (string.IsNullOrWhiteSpace(phone_number) && string.IsNullOrWhiteSpace(idcard) && string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["SEARCH_CRITERIA"];
                return View("Index");
            }

            if (!string.IsNullOrWhiteSpace(idcard))
            {
                int ch_length = idcard.Length;
                if (ch_length > 13)
                {
                    ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["IDCARD_13_DIGITS"];
                    return View("Index");
                }
                string idCardRegExp = @"\d{" + ch_length.ToString() + ",13}";
                if (!System.Text.RegularExpressions.Regex.IsMatch(idcard, idCardRegExp))
                {
                    ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["IDCARD_IS_NUMBER"];
                    return View("Index");
                }
            }
            

            var pageNumber = page ?? 1;
            IEnumerable<Customer> account_mobiles;
            string delete_status = FreebieStatus.MobileDeleted();
            using (var db = new EchoContext())
            {
                account_mobiles = from am in db.AccountMobiles
                                      join a in db.Accounts on am.Account_Id equals a.Account_Id 
                                      let idcard_no = (a.Identification_Number == null ? "" : a.Identification_Number)
                                      let Email = (a.User_Name == null ? "" : a.User_Name)
                                      where !am.Status_Cd.Equals(delete_status)
                                      select new Customer {
                                          Account_Id = a.Account_Id, 
                                          First_Name = a.First_Name,
                                          Last_Name = a.Last_Name,
                                          Email = Email,
                                          Identification_Number = idcard_no,
                                          Status_Cd = a.Status_Cd,
                                          Phone_Number = am.Mobile_Number};

                if (!string.IsNullOrWhiteSpace(phone_number))
                {
                    account_mobiles = account_mobiles.Where(x => x.Phone_Number.ToString().Contains(phone_number));
                }
                if (!string.IsNullOrWhiteSpace(idcard))
                {
                    account_mobiles = account_mobiles.Where(x => x.Identification_Number.Contains(idcard));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    account_mobiles = account_mobiles.Where(x => x.Email.Contains(email));
                }
                int pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["StaffPerPage"]);
                account_mobiles = account_mobiles.ToList().ToPagedList(pageNumber, pageSize);
                ViewBag.PageStaffs = account_mobiles;
                ViewBag.pageNumber = pageNumber;
                ViewBag.pageSize = pageSize;

                ViewBag.PhoneNumber = phone_number;
                ViewBag.Email = email;
                ViewBag.Identification_Number = idcard;

                return View(account_mobiles);
            }
           
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult AcctInfoByAgent(int Account_Id)
        {
            using (var db = new EchoContext())
            {
                Account account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
                if (account == null)
                {
                    return HttpNotFound();
                }
                string delete_status = FreebieStatus.MobileDeleted();
                string primary_number = "";
                string secondary_number = "";

                var account_numbers = db.AccountMobiles.Where(x => x.Account_Id.Equals(account.Account_Id)).Where(x => !x.Status_Cd.Equals(delete_status)).OrderByDescending(x => x.Primary_Flag);
                IEnumerable<AccountMobile> list_account_numbers = account_numbers.ToList();
                foreach (var number in list_account_numbers)
                {
                    if (number.Primary_Flag)
                    {
                        primary_number = number.Mobile_Number;
                    }
                    else
                    {
                        secondary_number += number.Mobile_Number;
                        if (number != list_account_numbers.Last() && (list_account_numbers.Count() - 1) > 0)
                        {
                            secondary_number += ", ";
                        }

                    }
                }
                //IEnumerable<AccountQuota> aqs = db.AccountQuotas.Where(x => x.Account_Id.Equals(account.Account_Id));
                //if (aqs.Count() > 0)
                //{
                //    Quota q = aqs.Last().Quota;
                //    ViewBag.Quota = q;
                //    ViewBag.Remaining = q.Quota_Freq_Val;
                //}
                ViewBag.PNumber = primary_number;
                ViewBag.SNumber = secondary_number;
                var today = DateTime.Now;
                AccountQuotaUsedCur qu = db.AccountQuotaUsedCurs.Where(x => x.Date.Equals(today.Date)).Where(x => x.Account_Id == account.Account_Id).SingleOrDefault();

                if (qu != null)
                {
                    int remaining = (Convert.ToInt16(qu.Quota_Freq_Val) - Convert.ToInt16(qu.Quota_Freq_Used_Val));
                    if (remaining < 0)
                    {
                        remaining = 0;
                    }
                    ViewBag.Remaining = remaining;
                    ViewBag.Freq = qu.Quota_Freq_Val;
                    ViewBag.Dur = qu.Quota_Dur_Val;
                }
                init_status(account);
                ViewBag.Account_Id = Account_Id;
                return View(account);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult AddNumberByAgent(int Account_Id)
        {
            ViewBag.ValidNumber = false;
            ViewBag.Account_Id = Account_Id;
            return View();
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult VerifySubrNumber(int Account_Id)
        {
            string phoneNumber = Request.Form["PhoneNumber"];
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.Path = "../SearchCustByAgent/ViewNumberByAgent?Account_Id=" + Account_Id.ToString();
            ViewBag.Account_Id = Account_Id;
            int result = CustomValidate.ValidateNumber(phoneNumber);
            ViewBag.ValidNumber = false;

            switch (result)
            {
                case 0:
                    ViewBag.Type = 1;
                    ModelState.AddModelError("PhoneNumber", System.Configuration.ConfigurationManager.AppSettings["Account010"]);
                    return View("AddNumberByAgent");
                case 1:
                        ViewBag.ValidNumber = true;                    
                        string otp = OTPHandler.SendOTPReg(phoneNumber);
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
                        AddCookie("AcctByAgent", new string[] { "phone_number" }, new string[] { phoneNumber });
                        return View("AddNumberByAgent");
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
                default:
                        ViewBag.Type = 1;
                        return View("AddNumberByAgent");
            
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult AddNumberByAgent(int Account_Id, string phoneNumber)
        {
            string password = Request.Form["Password"];
            phoneNumber = GetCookie("AcctByAgent", "phone_number");
            ViewBag.Account_Id = Account_Id;
            bool flag = true;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                ViewBag.ValidNumber = false;
                ViewBag.PhoneNumber = "";
                flag = false;
            }

            ViewBag.PhoneNumber = phoneNumber;

            string otp = Request.Form["Password"];

            if (flag && (string.IsNullOrEmpty(otp) || otp.Length < 4))
            {
                ViewBag.ValidNumber = true;
                ViewBag.Error = true;
                ViewBag.ErrorMessage = System.Configuration.ConfigurationManager.AppSettings["Validate010"];
                flag = false;
            }
            using (var db = new EchoContext())
            {
                if (flag)
                {
                    int result = OTPHandler.ValidateOTP(phoneNumber, otp);
                    switch (result)
                    {
                        case 0: 
                            Account account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
                            if (account == null)
                            {
                                return HttpNotFound();
                            }
                            AccountMobile am = db.AccountMobiles.Where(x => x.Account_Id.Equals(account.Account_Id)).Where(x => x.Mobile_Number.Equals(phoneNumber)).SingleOrDefault();
                            bool first_create = false;

                            if (am == null)
                            {
                                am = new AccountMobile();
                                am.Account_Id = account.Account_Id;
                                am.Status_Cd = FreebieStatus.MobileActive();
                                am.Mobile_Number = phoneNumber;
                                am.Primary_Flag = false;
                                am.Created_Dttm = DateTime.Now;
                                am.Updated_Dttm = DateTime.Now;
                                first_create = true;
                            }

                            if (first_create)
                            {
                                db.AccountMobiles.Add(am);
                            }
                            else
                            {
                                am.Status_Cd = FreebieStatus.MobileActive();
                                am.Created_Dttm = DateTime.Now;
                                am.Updated_Dttm = DateTime.Now;
                                db.Entry(am).State = EntityState.Modified;
                            }


                            OTP otp_request = db.OTPs.SingleOrDefault(x => x.PhoneNumber.Equals(phoneNumber));
                            if (otp_request != null)
                            {
                                db.OTPs.Remove(otp_request);
                            }
                            db.SaveChanges();
                            FreebieEvent.UpdateMobile(account, phoneNumber, "A03", Permission.cust_numbers_page_id);
                            RemoveCoookie("AcctByAgent");
                            return RedirectToAction("ViewNumberByAgent", new { Account_Id = Account_Id});
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
                            RemoveCoookie("AcctByAgent");
                            break;
                        case 3:
                            ViewBag.ValidNumber = false;
                            ViewBag.PhoneNumber = "";
                            ViewBag.ResetOTP = System.Configuration.ConfigurationManager.AppSettings["Otp04"];
                            RemoveCoookie("AcctByAgent");
                            break;
                        default:
                            break;
                    }

                }
            }
            ViewBag.ShowPwd = true;
            return View();
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult RemoveNumberByAgent(string phone_number)
        {
            int account_id = 0;
            using (var db = new EchoContext())
            {
                string delete_status = FreebieStatus.MobileDeleted();
                AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phone_number)).Where(x => !x.Status_Cd.Equals(delete_status)).SingleOrDefault();
                if (am == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    am.Status_Cd = FreebieStatus.MobileDeleted();
                    am.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                    am.Updated_Dttm = DateTime.Now;
                    account_id = am.Account_Id;
                    Account account = db.Accounts.SingleOrDefault(X => X.Account_Id == account_id);
                    FreebieEvent.UpdateMobile(account, phone_number, "A05", Permission.cust_numbers_page_id);
                    db.SaveChanges();
                }
                return RedirectToAction("ViewNumberByAgent", new { Account_Id = account_id}); 
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult ChangePrimaryByAgent(string phone_number)
        {
            int account_id = 0;
            string delete_status = FreebieStatus.MobileDeleted();
            using (var db = new EchoContext())
            {
                AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phone_number)).Where(x => !x.Status_Cd.Equals(delete_status)).SingleOrDefault();

                if (am == null) { return HttpNotFound(); }

                account_id = am.Account_Id;
                Account account = db.Accounts.SingleOrDefault(x => x.Account_Id == account_id);
                IEnumerable<AccountMobile> account_numbers = db.AccountMobiles.Where(x => x.Account_Id.Equals(account_id)).Where(x => !x.Status_Cd.Equals(delete_status));

                foreach (var number in account_numbers)
                {
                    if (number.Mobile_Number.Equals(phone_number))
                    {
                        number.Primary_Flag = true;
                    }
                    else
                    {
                        number.Primary_Flag = false;
                    }
                    number.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                    number.Updated_Dttm = DateTime.Now;

                }

                db.SaveChanges();
                FreebieEvent.UpdateMobile(account, phone_number, "A04", Permission.cust_numbers_page_id);
            }
            

            return RedirectToAction("ViewNumberByAgent", new { Account_Id = account_id });
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateAcctInfoByAgent(int account_id)
        {
            using (var db = new EchoContext())
            {
                Account account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(account_id));
                if (account == null)
                {
                    return HttpNotFound();
                }
                Account old_account = account;
                string status_cd = Request.Form["Statuses"];
                string old_status_cd = account.Status_Cd;
                account.Status_Cd = status_cd;
                if (old_status_cd.Equals("AP") && status_cd.Equals("ACD"))
                {
                    SqlParameter output = new SqlParameter("acstatus", SqlDbType.Int);
                    output.Direction = ParameterDirection.Output;

                    SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                    date.Value = DateTime.Now;

                    //update account_activation
                    string sql_string =
                         "declare @tbt table (acstatus int)" +
                         "UPDATE Account_Activation " +
                         "SET No_Manual_Activation = No_Manual_Activation + 1, " +
                         "No_Activation_Acc = No_Activation_Acc + 1, " +
                         "No_Activation_Pending = No_Activation_Pending - 1 " +
                         "OUTPUT CASE WHEN (deleted.No_Activation + 1 > deleted.No_Max_Activation) THEN 1 ELSE 0 END " +
                         " into @tbt " +
                         "WHERE [Date] = @today " +
                         "select @acstatus = acstatus from @tbt";

                    db.Database.ExecuteSqlCommand(sql_string, date, output);
                    int result = Convert.ToInt16(output.Value);
                }
                DateTime now = DateTime.Now;
                account.Updated_Dttm = now;
                account.Activation_Dttm = now;
                account.Updated_By = Session["User_No"].ToString();
                db.SaveChanges();
                if (!old_status_cd.Equals(status_cd))
                {
                    FreebieEvent.AccountUpdateEvent(account, status_cd, "Status", Permission.cust_profile_page_id);
                }
                
            }
            return RedirectToAction("AcctInfoByAgent", new { Account_Id = account_id });
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ViewNumberByAgent(int Account_Id)
        {
            using (var db = new EchoContext())
            {
                var account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
                if (account == null)
                {
                    return HttpNotFound();
                }
                string status = FreebieStatus.MobileActive();
                var account_numbers = db.AccountMobiles.Where(x => x.Account_Id.Equals(account.Account_Id)).Where(x => x.Status_Cd.Equals(status)).OrderByDescending(x => x.Primary_Flag);
                ViewBag.Account_Id = Account_Id;
                return View(account_numbers.ToList());
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ViewAcctProfileByAgent(int Account_Id)
        {
            ViewBag.Account_Id = Account_Id;
            var db = new EchoContext();
                Account account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
                if (account == null)
                {
                    return HttpNotFound();
                }
                var account_interest = db.AccountInterests.Where(x => x.Account_Id.Equals(account.Account_Id)).SingleOrDefault();
                if (account_interest == null)
                {
                    account_interest = new AccountInterest();
                }

                List<string> interest_arrs = load_interest(account_interest);

                AccountQuota account_quota = db.AccountQuotas.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
                Quota quota = new Quota();
                if (account_quota == null)
                {
                    account_quota = new AccountQuota();
                }
                else
                {
                    quota = account_quota.Quota;
                }

                string district = "";
                if (!string.IsNullOrWhiteSpace(account.AreaCode))
                {
                    Zipcode zipcode = db.Zipcodes.SingleOrDefault(x => x.AreaCode.Equals(account.AreaCode));
                    if (zipcode != null)
                    {
                        district = zipcode.District;
                    }
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
                ViewBag.Quota_Freq_Val = Convert.ToInt16(quota.Quota_Freq_Val);
                ViewBag.Quota_Dur_Val = Convert.ToInt16(quota.Quota_Dur_Val);
                if (quota.Quota_Cd != null)
                {
                    if (quota.Quota_Cd.Equals("Q0001"))
                    {
                        ViewBag.Score = 3;
                    }
                    else
                    {
                        if (quota.Quota_Cd.Equals("Q0002"))
                        {
                            ViewBag.Score = 6;
                        }
                        else
                        {
                            if (quota.Quota_Cd.Equals("Q0003"))
                            {
                                ViewBag.Score = 8;
                            }
                            else
                            {
                                ViewBag.Score = 0;
                            }
                        }
                    }
                }
                ViewBag.District = district;
                
                ViewBag.InterestSelected = interest_arrs;
                init_dropdown(account);
                return View(account);
           
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateAcctProfileByAgent(int Account_Id)
        {
            var db = new EchoContext();
            ViewBag.Account_Id = Account_Id;
            Account account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
            if (account == null) { return HttpNotFound(); }
            var account_interest = db.AccountInterests.Where(x => x.Account_Id.Equals(account.Account_Id)).SingleOrDefault();

            if (account_interest == null)
            {
                account_interest = new AccountInterest();
            }

            List<string> interest_arrs = load_interest(account_interest);
            ViewBag.InterestSelected = interest_arrs;

            init_dropdown(account);

            AccountQuota account_quota = db.AccountQuotas.SingleOrDefault(x => x.Account_Id == Account_Id);
            Quota quota = new Quota();
            if (account_quota == null)
            {
                account_quota = new AccountQuota();
            }
            else
            {
                quota = account_quota.Quota;
            }

            ViewBag.Quota_Freq_Val = Convert.ToInt16(quota.Quota_Freq_Val);
            ViewBag.Quota_Dur_Val = Convert.ToInt16(quota.Quota_Dur_Val);

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

            return View(account);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateAcctProfileByAgent(Account account)
        {
            var db = new EchoContext();
            account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(account.Account_Id));
            string old_idcard = account.Identification_Number == null ? string.Empty : account.Identification_Number.Trim();
            //Account old_account = account;

            var selected_interests = Request.Form["selectedInterests"];
            if (string.IsNullOrWhiteSpace(selected_interests))
            {
                selected_interests = "";
            }
            var agree_flag = Request.Form["Agree"];
            ViewBag.NotAgree = "";
            if (account == null) { return HttpNotFound(); }
            ViewBag.Account_Id = account.Account_Id;
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

            if (ModelState.ContainsKey("User_Name"))
                ModelState["User_Name"].Errors.Clear();
            if (ModelState.ContainsKey("User_Name"))
                ModelState["Password"].Errors.Clear();

            var form_vals = Request.Form;

            if (CustomValidate.ValidateZipcode(form_vals["ZipCode"]) != 1)
            {
                ModelState.AddModelError("ZipCode", System.Configuration.ConfigurationManager.AppSettings["Account023"]);
            }

            if (string.IsNullOrWhiteSpace(form_vals["First_Name"]))
            {
                ModelState.AddModelError("First_Name", System.Configuration.ConfigurationManager.AppSettings["Account003"]);
            }

            if (string.IsNullOrWhiteSpace(form_vals["Last_Name"]))
            {
                ModelState.AddModelError("Last_Name", System.Configuration.ConfigurationManager.AppSettings["Account004"]);
            }

            if (string.IsNullOrEmpty(form_vals["Day_Of_Birth"]))
            {
                account.Day_Of_Birth = null;
            }
            else
            {
                account.Day_Of_Birth = Convert.ToByte(form_vals["Day_Of_Birth"]);
            }
            if (string.IsNullOrEmpty(form_vals["Month_Of_Birth"]))
            {
                account.Month_Of_Birth = null;
            }
            else
            {
                account.Month_Of_Birth = Convert.ToByte(form_vals["Month_Of_Birth"]);
            }
            if (string.IsNullOrEmpty(form_vals["Year_Of_Birth"]))
            {
                account.Year_Of_Birth = null;
            }
            else
            {
                account.Year_Of_Birth = Convert.ToInt16(form_vals["Year_Of_Birth"]);
            }
            bool no_child = true;
            if (!string.IsNullOrEmpty(form_vals["Children_Flag"]))
            {
                if (form_vals["Children_Flag"].Equals("Y"))
                {
                    account.Children_Flag = "Y";
                    no_child = false;
                }
                else
                {
                    account.Children_Flag = "N";
                }
            }

            if (no_child || string.IsNullOrEmpty(form_vals["Year_Of_Birth_Child1"]))
            {
                account.Year_Of_Birth_Child1 = null;
            }
            else
            {
                account.Year_Of_Birth_Child1 = Convert.ToInt16(form_vals["Year_Of_Birth_Child1"]);
            }
            if (no_child || string.IsNullOrEmpty(form_vals["Year_Of_Birth_Child2"]))
            {
                account.Year_Of_Birth_Child2 = null;
            }
            else
            {
                account.Year_Of_Birth_Child2 = Convert.ToInt16(form_vals["Year_Of_Birth_Child2"]);
            }
            if (no_child || string.IsNullOrEmpty(form_vals["Year_Of_Birth_Child3"]))
            {
                account.Year_Of_Birth_Child3 = null;
            }
            else
            {
                account.Year_Of_Birth_Child3 = Convert.ToInt16(form_vals["Year_Of_Birth_Child3"]);
            }
            string idcard = form_vals["Identification_Number"] == null ? string.Empty : form_vals["Identification_Number"].Trim();
            if (!string.IsNullOrEmpty(idcard))
            {
                switch (CustomValidate.ValidateIndentification(idcard))
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
            }
            
            if (!string.IsNullOrEmpty(account.Children_Flag))
            {
                if (account.Children_Flag.Equals("Y"))
                {
                    if (account.Year_Of_Birth_Child1 == null)
                    {
                        ModelState.AddModelError("Year_Of_Birth_Child1", System.Configuration.ConfigurationManager.AppSettings["Account021"]);
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(form_vals["Day_Of_Birth"]) || string.IsNullOrWhiteSpace(form_vals["Month_Of_Birth"]) || string.IsNullOrWhiteSpace(form_vals["Year_Of_Birth"]))
            {
                ModelState.AddModelError("Day_Of_Birth", System.Configuration.ConfigurationManager.AppSettings["Account020"]);
            }
            else
            {
                byte day_of_birth = Convert.ToByte(form_vals["Day_Of_Birth"]);
                byte month_of_birth = Convert.ToByte(form_vals["Month_Of_Birth"]);
                int year_of_birth = Convert.ToInt32(form_vals["Year_Of_Birth"]);

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
            if (agree_flag == "true")
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        account.Updated_Dttm = DateTime.Now;
                        account.Updated_By = Session["User_No"].ToString();
                        UpdateModel(account);

                        string[] interests = new string[] { };

                        var aci = db.AccountInterests.Where(x => x.Account_Id.Equals(account.Account_Id)).SingleOrDefault();
                        bool flag = false;
                        if (aci == null)
                        {
                            aci = new AccountInterest();
                            aci.Account_Id = account.Account_Id;
                            flag = true;
                        }
                        if (selected_interests == null)
                        {
                            selected_interests = "";
                        }

                        

                        interests = selected_interests.Split(',');

                        aci.I01_Food_Dining = interests.Contains("I01");
                        aci.I02_Night_Life = interests.Contains("I02");
                        aci.I03_Entertainment = interests.Contains("I03");
                        aci.I04_Music_Movie = interests.Contains("I04");
                        aci.I05_Sports_Fitness = interests.Contains("I05");
                        aci.I06_Shopping_Fashion = interests.Contains("I06");
                        aci.I07_Health_Beauty = interests.Contains("I07");
                        aci.I08_Travel = interests.Contains("I08");
                        aci.I09_Pets = interests.Contains("I09");
                        aci.I10_Kids_Children = interests.Contains("I10");
                        aci.I11_Home_Living = interests.Contains("I11");
                        aci.I12_Finance_Investment = interests.Contains("I12");
                        aci.I13_Technology_Gadget = interests.Contains("I13");
                        aci.I14_Auto = interests.Contains("I14");

                        if (flag)
                        { db.AccountInterests.Add(aci); }
                        else
                        { db.Entry(aci).State = EntityState.Modified; }
                        db.SaveChanges();

                        Account edited_account = db.Accounts.SingleOrDefault(x => x.Account_Id == account.Account_Id);
                        Quota select_quota = QuotaCalculation.Calculate(edited_account, selected_interests);

                        AccountQuota aq = db.AccountQuotas.SingleOrDefault(x => x.Account_Id == edited_account.Account_Id);
                        if (aq != null)
                        { db.AccountQuotas.Remove(aq); }
                        AccountQuota new_aq = new AccountQuota();
                        new_aq.Account = edited_account;
                        new_aq.Quota_Cd = select_quota.Quota_Cd;
                        db.AccountQuotas.Add(new_aq);

                        db.SaveChanges();

                        if (!old_idcard.Equals(idcard))
                        {
                            FreebieEvent.AccountUpdateEvent(account, idcard, "Idcard", Permission.cust_profile_page_id);
                        }
                        else
                        {
                            FreebieEvent.AccountUpdateEvent(account, null, null, Permission.cust_profile_page_id);
                        }
                        

                        return RedirectToAction("after_create_account", new { account_id = account.Account_Id, selected_interests = selected_interests });
                        //return RedirectToAction("ViewAcctProfileByAgent", new { Account_Id = account.Account_Id});
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }
            else
            {
                ViewBag.NotAgree = System.Configuration.ConfigurationManager.AppSettings["Account006"];
            }
            var account_interest = db.AccountInterests.Where(x => x.Account_Id.Equals(account.Account_Id)).SingleOrDefault();

            if (account_interest == null)
            {
                account_interest = new AccountInterest();
            }

            //List<string> interest_arrs = load_interest(account_interest);
            string[] selected_interest_arrs = selected_interests.Split(',');
            ViewBag.InterestSelected = selected_interest_arrs.ToList();
            init_dropdown(account);
            return View(account);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateUsernameByAgent(int Account_Id)
        {
            var db = new EchoContext();

            var account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(Account_Id));
            if (account == null)
            {
                return HttpNotFound();
            }
            ViewBag.Account_Id = Account_Id;

            return View(account);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateUsernameByAgent(Account account)
        {
            var db = new EchoContext();
            account = db.Accounts.SingleOrDefault(x => x.Account_Id.Equals(account.Account_Id));
            if (account == null)
            {
                return HttpNotFound();
            }
            ViewBag.Account_Id = account.Account_Id;
            var new_username = Request.Form["New_User_Name"];
            var confirm_username = Request.Form["Confirm_User_Name"];
            ViewBag.New_User_Name = new_username;
            ViewBag.Confirm_User_Name = confirm_username;

            if (!string.IsNullOrWhiteSpace(account.User_Name))
            {
                string strPattern = CustomValidate.EmailRegEx();
                if (!System.Text.RegularExpressions.Regex.IsMatch(new_username, strPattern))
                {
                    ModelState.AddModelError("New_User_Name", System.Configuration.ConfigurationManager.AppSettings["Account016"]);
                    return View(account);
                }
                var check_ac = db.Accounts.Where(x => x.User_Name.Equals(new_username)).SingleOrDefault();
                if (check_ac != null)
                {
                    ModelState.AddModelError("New_User_Name", System.Configuration.ConfigurationManager.AppSettings["Account015"]);
                    return View(account);
                }
                if (new_username.ToLower() == confirm_username.ToLower())
                {
                    account.User_Name = new_username;
                    account.Updated_Dttm = DateTime.Now;
                    account.Updated_By = Session["User_No"].ToString();
                    db.Entry(account).State = EntityState.Modified;
                    db.SaveChanges();
                    FreebieEvent.AccountUpdateEvent(account, null, null, Permission.update_cust_username_page_id);
                    ViewBag.Path = "AcctInfoByAgent?Account_Id=" + account.Account_Id.ToString();
                    return View("ChangeUsernameResult");
                }
                else
                {
                    ModelState.AddModelError("Confirm_User_Name", System.Configuration.ConfigurationManager.AppSettings["Account017"]);
                }
            }
            else
            {
                ModelState.AddModelError("New_User_Name", System.Configuration.ConfigurationManager.AppSettings["Account024"]);
            }
            
            

            return View(account);
        }

        private void init_status(Account account)
        {
            List<SelectListItem> statuses = new List<SelectListItem>();

            using(var db = new EchoContext())
            {
                IEnumerable<Status> freebie_status = db.Statuses.Where(x => x.Status_Type.Equals("Account"));
                var current_item = new SelectListItem()
                {
                    Text = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals(account.Status_Cd)).Status_Name_Th,
                    Value = account.Status_Cd,
                    Selected = true
                };
                statuses.Add(current_item);

                Status select_status = null;
                switch (account.Status_Cd)
                { 
                    case "AC":
                        select_status = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals("HS"));
                        break;
                    case "ACD":
                        select_status = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals("HS"));
                        break;
                    case "AP":
                        select_status = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals("ACD"));
                        break;
                    case "HS":
                        select_status = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals("AC"));
                        break;
                    case "IA":
                        select_status = freebie_status.SingleOrDefault(x => x.Status_Cd.Equals("HS"));
                        break;
                    default:
                        break;
                }

                if (select_status != null)
                { 
                    var item = new SelectListItem()
                    {
                        Text = select_status.Status_Name_Th,
                        Value = select_status.Status_Cd,
                        Selected = false
                    };

                    statuses.Add(item);
                }

                ViewBag.Statuses = statuses;
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
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }

        private List<string> load_interest(AccountInterest aci)
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

        private void init_dropdown(Account account)
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
                ViewBag.Interests = db.Interests.ToList();
 
                ViewBag.Year_Of_Birth_Child1 = child1_years;
                ViewBag.Year_Of_Birth_Child2 = child2_years;
                ViewBag.Year_Of_Birth_Child3 = child3_years;
            
        }
        public ActionResult after_create_account(int account_id, string selected_interests)
        {
            if (selected_interests == null)
            {
                selected_interests = "";
            }
            using (var echo = new EchoContext())
            {
                Account account = echo.Accounts.SingleOrDefault(x => x.Account_Id.Equals(account_id));

                if (account != null)
                {
                    Quota select_quota = QuotaCalculation.Calculate(account, selected_interests);
                    AccountQuota aq = echo.AccountQuotas.SingleOrDefault(x => x.Account_Id.Equals(account_id));
                    if (aq != null)
                    {
                        echo.AccountQuotas.Remove(aq);
                        echo.SaveChanges();
                    }
                    AccountQuota new_aq = new AccountQuota();

                    new_aq.Account_Id = account_id;
                    new_aq.Quota_Cd = select_quota.Quota_Cd;
                    echo.AccountQuotas.Add(new_aq);
                    echo.SaveChanges();
                }
                return RedirectToAction("ViewAcctProfileByAgent", new { Account_Id = account.Account_Id }); 
            }
        }
    }
}
