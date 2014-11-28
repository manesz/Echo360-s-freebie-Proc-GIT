using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freebie.Models;
using Freebie.Libs;
using Freebie.ViewModels;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Data.Entity.Infrastructure;

namespace Freebie.Libs
{
    public static class ActivationSMS
    {

        // for test
        //public static void seed_data(int num)
        //{
        //    using (var db = new EchoContext())
        //    {

        //        for (int i = 1; i <= num; i++)
        //        {
        //            Account a = new Account();
        //            a.First_Name = PasswordGenerator.Get();
        //            a.Last_Name = PasswordGenerator.Get();
        //            a.Day_Of_Birth = 1;
        //            a.Month_Of_Birth = 2;
        //            a.Year_Of_Birth = 1990;
        //            a.Gender_Cd = "M";
        //            a.Dummy_Flag = "0";
        //            a.Activation_Dttm = DateTime.Now;
        //            a.Status_Cd = "AC";
        //            db.Accounts.Add(a);

        //            AccountMobile pn = new AccountMobile();
        //            pn.Account = a;
        //            pn.Mobile_Number = "08" + i.ToString() + "100" + string.Format("{0:0000}", i);
        //            pn.Primary_Flag = true;
        //            pn.Status_Cd = "AC";

        //            db.AccountMobiles.Add(pn);

        //            AccountMobile sn = new AccountMobile();
        //            sn.Account = a;
        //            sn.Mobile_Number = "09" + i.ToString() + "100" + string.Format("{0:0000}", i);
        //            sn.Primary_Flag = false;
        //            sn.Status_Cd = "AC";

        //            db.AccountMobiles.Add(sn);
        //            db.SaveChanges();
        //        }

        //        for (int j = 1; j <= num; j++)
        //        {
        //            Account a = new Account();
        //            a.First_Name = PasswordGenerator.Get();
        //            a.Last_Name = PasswordGenerator.Get();
        //            a.Day_Of_Birth = 1;
        //            a.Month_Of_Birth = 2;
        //            a.Year_Of_Birth = 1990;
        //            a.Gender_Cd = "M";
        //            a.Dummy_Flag = "0";
        //            a.Activation_Dttm = DateTime.Now.AddDays(-1);
        //            a.Status_Cd = "AC";

        //            db.Accounts.Add(a);

        //            AccountMobile pn = new AccountMobile();
        //            pn.Account = a;
        //            pn.Mobile_Number = "08" + j.ToString() + "110" + string.Format("{0:0000}", j);
        //            pn.Primary_Flag = true;
        //            pn.Status_Cd = "AC";

        //            db.AccountMobiles.Add(pn);

        //            AccountMobile sn = new AccountMobile();
        //            sn.Account = a;
        //            sn.Mobile_Number = "09" + j.ToString() + "110" + string.Format("{0:0000}", j);
        //            sn.Primary_Flag = false;
        //            sn.Status_Cd = "AC";

        //            db.AccountMobiles.Add(sn);

        //            db.SaveChanges();
        //        }
               
        //    }
            
        //}

        public static IEnumerable<AccountSMS> get_accounts()
        {
            
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);


            IEnumerable<AccountSMS> accounts;
            var db = new EchoContext();

            accounts = from am in db.AccountMobiles
                       join a in db.Accounts on am.Account_Id equals a.Account_Id
                       where am.Primary_Flag  && a.Activation_Dttm != null && (a.Registration_Dttm != a.Activation_Dttm) && (a.Activation_Dttm >= today && a.Activation_Dttm < tomorrow)
                       select new AccountSMS
                       {
                           Account_Id = a.Account_Id,
                           Activation_Dttm = a.Activation_Dttm,
                           Mobile_Number = am.Mobile_Number
                       };
          
            return accounts;
        }

        public static void send_sms()
        {
            
            var accounts = get_accounts();
            List<string> retry_numbers = new List<string>();

            int wait = 5000;
            if (!string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_TIMESPAN"]))
            {
                wait = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_TIMESPAN"]);
            }
            int retry_time = 3;
            if (!string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_RETRY"]))
            {
                retry_time = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_RETRY"]);
            }
            int retry_wait = 5000;
            if (!string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_RETRY_TIMESPAN"]))
            {
                retry_wait = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_RETRY_TIMESPAN"]);
            }

            // first try
            Debug.WriteLine("There are " + accounts.Count().ToString() + " records");
            foreach (var account in accounts)
            {
                string result = Message.Notify_Account(account.Mobile_Number);                
                //Debug.WriteLine(result + " Send to " + account.Mobile_Number + " at " + DateTime.Now.ToString());
                try
                {
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        if (!result.ToLower().Equals("status=0"))
                        {
                            retry_numbers.Add(account.Mobile_Number);
                        }
                    }
                }
                catch (Exception) { continue; }

                Thread.Sleep(wait);
            }

            // retry loop
            foreach (var number in retry_numbers)
            {
                bool continue_loop = true;
                int retry_count = 1;

                while (retry_count <= retry_time && continue_loop)
                {
                    string result = Message.Notify_Account(number);

                    Thread.Sleep(retry_wait);
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            if (!result.ToLower().Equals("status=0"))
                            {
                                //Debug.WriteLine("retry " + number + " " + retry_count.ToString());
                                retry_count += 1;
                            }
                            else
                            {
                                // exit
                                //Debug.WriteLine("---Exit " + number);
                                continue_loop = false;
                            }
                        }
                    }
                    catch (Exception) { continue; }  
                }
                Thread.Sleep(wait);
            }
        }
    }
}