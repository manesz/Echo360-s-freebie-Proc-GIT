using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Models;
using System.Data;
using Freebie.ViewModels;
using Freebie.Libs;

namespace Freebie.Areas.Backend.Controllers
{
    public class AdminConfigController : Controller
    {
        //
        // GET: /Backend/AdminConfig/
        //private EchoContext db = new EchoContext();

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult BaseQuota()
        {
            using (var db = new EchoContext())
            {
                IEnumerable<Quota> quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B"));
                return View(quotas.ToList());
            }     
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateBaseQuota()
        {
            using (var db = new EchoContext())
            {
                IEnumerable<Quota> quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B"));
                return View(quotas.ToList());
            }
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult FreeTrialQuota()
        {
            using (var db = new EchoContext())
            {
                TrialQuota tq = new TrialQuota();

                var today = DateTime.Now.Date;
                AccountTrial today_acctt = db.AccountTrials.SingleOrDefault(x => x.Date.Equals(today));


                if (today_acctt == null)
                {
                    today_acctt = new AccountTrial();
                }

                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();

                if (ac == null)
                {
                    ac = new AdminConfiguration();
                }

                tq.no_trial_used = today_acctt.No_Trial_Used;
                tq.no_trial_acc = today_acctt.No_Trial_Used_Acc;
                tq.trial_limit_total = ac.Trial_Limit_Total;
                tq.trial_dur_val = ac.Trial_Dur_Val;
                tq.trial_enable_flag = ac.Trial_Enable_Flag;

                return View(tq);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateBaseQuota(string state)
        {
            using (var db = new EchoContext())
            {
                IEnumerable<Quota> quotas = db.Quotas.Where(x => x.Quota_Type_Cd.Equals("B"));
                string user_no = Session["User_No"].ToString();
                foreach (var item in quotas)
                {
                    string fv = Request.Form["Quota_Freq_Val_" + item.Quota_Cd];
                    string dv = Request.Form["Quota_Dur_Val_" + item.Quota_Cd];

                    try
                    {
                        item.Quota_Freq_Val = Convert.ToByte(fv);
                        item.Quota_Dur_Val = Convert.ToByte(dv);
                        item.Updated_By = user_no;
                        item.Updated_Dttm = DateTime.Now;
                        db.Entry(item).State = EntityState.Modified;
                    }
                    catch
                    {
                        return View(quotas);
                    }

                }
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.base_quota_page_id, "A04");
                return RedirectToAction("BaseQuota");
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateFreeTrialQuota()
        {
            using (var db = new EchoContext())
            {
                TrialQuota tq = new TrialQuota();
                var today = DateTime.Now.Date;
                AccountTrial today_acctt = db.AccountTrials.SingleOrDefault(x => x.Date.Equals(today));

                if (today_acctt == null)
                {
                    today_acctt = new AccountTrial();
                }

                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();

                if (ac == null)
                {
                    ac = new AdminConfiguration();
                }

                tq.no_trial_used = today_acctt.No_Trial_Used;
                tq.no_trial_acc = today_acctt.No_Trial_Used_Acc;
                tq.trial_limit_total = ac.Trial_Limit_Total;
                tq.trial_dur_val = ac.Trial_Dur_Val;
                tq.trial_enable_flag = ac.Trial_Enable_Flag;

                return View(tq);
            }
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateFreeTrialQuota(TrialQuota tq)
        {
            using (var db = new EchoContext())
            {
                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();
                string user_no = Session["User_No"].ToString();
                ac.Trial_Limit_Total = Convert.ToInt32(tq.trial_limit_total);
                ac.Trial_Dur_Val = Convert.ToInt32(tq.trial_dur_val);
                ac.Trial_Enable_Flag = Convert.ToBoolean(tq.trial_enable_flag);
                ac.Updated_By = user_no;
                ac.Updated_Dttm = DateTime.Now;

                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.free_trial_page_id, "A04");
                return RedirectToAction("FreeTrialQuota");
            }       
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ActivationLimit()
        {
            using (var db = new EchoContext())
            {
                ActivationLimit al = new ActivationLimit();

                var date = DateTime.Now.Date;

                AccountActivation aa = db.AccountActivations.Where(x => x.Date.Equals(date)).SingleOrDefault();

                if (aa == null)
                {
                    aa = new AccountActivation();
                }

                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();

                if (ac == null)
                {
                    ac = new AdminConfiguration();
                }

                al.no_activation = aa.No_Activation;
                al.no_activation_pending = aa.No_Activation_Pending;
                al.no_activation_acc = aa.No_Activation_Acc;
                al.no_activation_limit_total = ac.No_Activation_Limit_Total;
                al.no_activation_limit_daily = ac.No_Activation_Limit_Daily;


                return View(al);
            
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateActivationLimit()
        {
            using (var db = new EchoContext())
            {
                ActivationLimit al = new ActivationLimit();

                var date = DateTime.Now.Date;

                AccountActivation aa = db.AccountActivations.Where(x => x.Date.Equals(date)).SingleOrDefault();

                if (aa == null)
                {
                    aa = new AccountActivation();
                }

                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();

                if (ac == null)
                {
                    ac = new AdminConfiguration();
                }

                al.no_activation = aa.No_Activation;
                al.no_activation_pending = aa.No_Activation_Pending;
                al.no_activation_acc = aa.No_Activation_Acc;
                al.no_activation_limit_total = ac.No_Activation_Limit_Total;
                al.no_activation_limit_daily = ac.No_Activation_Limit_Daily;


                return View(al);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateActivationLimit(ActivationLimit al)
        {
            using (var db = new EchoContext())
            {
                AdminConfiguration ac = db.AdminConfigurations.SingleOrDefault();
                string user_no = Session["User_No"].ToString();
                ac.No_Activation_Limit_Total = Convert.ToInt32(al.no_activation_limit_total);
                ac.No_Activation_Limit_Daily = Convert.ToInt32(al.no_activation_limit_daily);
                ac.Updated_By = user_no;
                ac.Updated_Dttm = DateTime.Now;

                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.activation_page_id, "A04");
                return RedirectToAction("ActivationLimit");
            }
            
        }

        //[FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        //public ActionResult GetSchedulers()
        //{
            
        //    return View();
        //}

    }


}
