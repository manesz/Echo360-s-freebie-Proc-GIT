using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freebie.Models;
using System.Diagnostics;

namespace Freebie.Libs
{
    static class CustomValidate
    {
        //private static EchoContext db = new EchoContext();

        public static bool is_service_disabled()
        {
           

            using (var val_db = new EchoContext())
            {
                bool result = false;
                AdminConfiguration ac = val_db.AdminConfigurations.SingleOrDefault();
                if (ac == null)
                {
                    ac = new AdminConfiguration();
                }

                DateTime utc_now = DateTime.UtcNow;
                //TimeZoneInfo bkk = TimeZoneInfo.FindSystemTimeZoneById("S.E. Asia Standard Time");
                DateTime now = utc_now.AddHours(7);
                TimeSpan disabled_start = (TimeSpan)ac.Regist_Disable_StartTime;
                TimeSpan disabled_end = (TimeSpan)ac.Regist_Disable_EndTime;
                if (disabled_end < disabled_start)
                {
                    disabled_end = disabled_end.Add(new TimeSpan(24, 0, 0));
                }
                TimeSpan disabled_span = disabled_end - disabled_start;
                DateTime start_at = new DateTime(now.Year, now.Month, now.Day, disabled_start.Hours, disabled_start.Minutes, disabled_start.Seconds);
                DateTime end_at = start_at.Add(disabled_span);

                if (start_at <= now && now <= end_at)
                {
                    result = true;
                }

                return result;
            }
            
        }

        public static int ValidateNumber(string number)
        {
            // return values
            // 0 required input or incorrent number format
            // 1 pass
            // 2 reserved for non-AIS validation
            // 3 number exists  not allow to register
            // 4 number exists inactive, re use
            // 5 number exists active, but allow user to enter username and password
            // 6 registeration disabled

            if (is_service_disabled())
            {
                return 6;
            }
            
            // Validate Format
            if (string.IsNullOrEmpty(number))
            { return 0; }
            string prefix_config = System.Configuration.ConfigurationManager.AppSettings["MOBILE_PREFIX"];

            prefix_config = prefix_config.Trim();
            string[] prefixes = prefix_config.Split(',');
            string allow_prefix = "";
            foreach (var pf in prefixes)
            { 
                string p = pf.Replace("0", "");
                allow_prefix += p;
            }
            string phoneRegExp = "";
            phoneRegExp = "^0[" +  allow_prefix + @"]\d{8}$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(number, phoneRegExp) || number.Length != 10)
            { return 0; }

            string delete_status = FreebieStatus.MobileDeleted();
            using (var val_db = new EchoContext())
            {
                AccountMobile check_mobile = val_db.AccountMobiles.Where(x => x.Mobile_Number.Equals(number)).Where(x => !x.Status_Cd.Equals(delete_status)).SingleOrDefault();
                // not ais return 2
                // Validate Existance
                if (check_mobile != null)
                {
                    if (check_mobile.Status_Cd.Trim().Equals("AC") || check_mobile.Status_Cd.Trim().Equals("ACD"))
                    {
                        Account account = check_mobile.Account;

                        if (account.Status_Cd.ToString().Trim().Equals(FreebieStatus.AccountPTUU()) || account.Status_Cd.ToString().Trim().Equals(FreebieStatus.AccountPTU()))
                        {
                            return 7;
                        }

                        if (string.IsNullOrWhiteSpace(account.User_Name))
                        {
                            return 5;
                        }

                        
                        return 3;
                    }
                    else
                    {
                        if (check_mobile.Status_Cd.Trim().Equals("IA"))
                        {
                            return 4;
                        }
                        return 3;
                    }
                }
                else
                {
                    return 1;
                }
            }
            
        }

        public static string EmailRegEx()
        {
            return "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z_])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
        }

        public static int ValidateUsername(string email, string cfm_email, string pwd, string cfm_pwd)
        {
            // Missing some value
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(cfm_email) || string.IsNullOrEmpty(cfm_pwd))
            { return 0; }

            //Format
            string strPattern = EmailRegEx();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, strPattern))
            { return 2; }

            if (pwd.Length < 6 || pwd.Length > 15)
            { return 3; }

            using (var val_db = new EchoContext())
            {
                //Existance
                var check_email = val_db.Accounts.SingleOrDefault(a => a.User_Name == email);
                if (check_email != null)
                { return 4; }

                // Value match
                if (email.ToLower() != cfm_email.ToLower())
                    return 5;
                if (pwd != cfm_pwd)
                    return 6;
            }
           

            return 1;
        }

        public static int ValidateZipcode(string zipcode)
        {
            if (string.IsNullOrEmpty(zipcode))
            { return 1; }

            string numberRegExp = @"(\d\d\d\d\d)";
            if (!System.Text.RegularExpressions.Regex.IsMatch(zipcode, numberRegExp))
            { return 0; }

            return 1;
        }
        public static int ValidateIndentification(string id_number)
        {
            int checksum = 0;
            if (string.IsNullOrEmpty(id_number))
            { return 1; }

            string numberRegExp = @"(\d\d\d\d\d\d\d\d\d\d\d\d\d)";
            if (!System.Text.RegularExpressions.Regex.IsMatch(id_number, numberRegExp))
            { return 0; }

            if (id_number.Length != 13)
            { return 2; }

            for (int i = 0; i < 12; i++)
            {
                checksum = checksum + (int)Char.GetNumericValue(id_number[i]) * (13 - i);             
            }

            checksum = checksum % 11;          
            if (checksum == 0) { checksum = 10; }           
            checksum = 11 - checksum;          
            if (checksum == 10) { checksum = 0; }           
            if (checksum != (int)Char.GetNumericValue(id_number[12]))
            { return 3; }

            return 1;
        }

        public static int ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            { return 0; }

            string strPattern = EmailRegEx();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, strPattern))
            { return 2; }

            using (var val_db = new EchoContext())
            {
                var check_email = val_db.Accounts.SingleOrDefault(a => a.User_Name == email);
                if (check_email == null)
                { return 3; }
            }

            return 1;
        }
    }
}