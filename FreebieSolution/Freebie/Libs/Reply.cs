using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using Freebie.Models;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace Freebie.Libs
{
    public class Reply : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string reply_str = "";
            var db = new EchoContext();
            SmsRegistrationLog sms_log = new SmsRegistrationLog();
            string sms_log_result = "";
            try
            {
                string keyword = context.Request.Form["keyword"] == null ? string.Empty : context.Request.Form["keyword"];
                string content = context.Request.Form["content"] == null ? string.Empty : context.Request.Form["content"];
                string mobile_no = context.Request.Form["mobile_no"] == null ? string.Empty : context.Request.Form["mobile_no"];
                //string msg = context.Request.Form["msg"] == null ? string.Empty : context.Request.Form["msg"];

                
                sms_log.Mobile_Number = mobile_no;
                sms_log.RQ_Msg = "-";
                sms_log.RQ_Keyword = keyword;
                sms_log.RQ_Content = content;
                

                int result = CustomValidate.ValidateNumber(mobile_no);
                bool flag = true;

                if (result != 1 && result != 4)
                {
                    flag = false;
                    if (result == 2 || result == 3 || result == 5)
                    {
                        reply_str = System.Configuration.ConfigurationManager.AppSettings["EXIST_NUMBER"];
                        sms_log_result = "Existing number";
                    }

                    if (result == 6)
                    {
                        reply_str = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                        sms_log_result = "Maintenance Period";
                    }
                }

                if (flag)
                {
                    if (IsValid(keyword, content))
                    {
                        string[] content_arrs = content.Split(' ');
                        string gender = content_arrs[0];
                        string dob = content_arrs[1];
                        string[] result_sp = new string[2];

                        byte day = Convert.ToByte(dob.Substring(0, 2));
                        byte month = Convert.ToByte(dob.Substring(2, 2));
                        int year = Convert.ToInt16(dob.Substring(4, 4));

                        year = year - 543;
                            #region transaction
                            var transactionOptions = new TransactionOptions();
                            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                            Account account = new Account();
                            DateTime timestamp = DateTime.Now;
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
                            {
                                var db_transaction = new EchoContext();
                                
                                account.Gender_Cd = gender.ToUpper();
                                account.Day_Of_Birth = day;
                                account.Month_Of_Birth = month;
                                account.Year_Of_Birth = year;
                                account.Channel_Cd = "SMS";
                                account.Created_By = System.Configuration.ConfigurationManager.AppSettings["CREATED_BY_SMS"];
                                account.Updated_By = System.Configuration.ConfigurationManager.AppSettings["CREATED_BY_SMS"];
                                account.First_Mobile_Number = mobile_no;
                                account.Created_Dttm = timestamp;
                                account.Updated_Dttm = timestamp;
                                account.Registration_Dttm = timestamp;

                                var today = DateTime.Now.Date;
                                Quota q = db_transaction.Quotas.Where(x => x.Quota_Type_Cd.Equals("B")).Where(x => x.Quota_Cd.Equals("Q0001")).SingleOrDefault();

                                #region account quota used cur
                                    AccountQuotaUsedCur aquc = new AccountQuotaUsedCur();
                                    aquc.Date = today.Date;
                                    aquc.Account = account;
                                    aquc.Quota_Freq_Used_Val = 0;
                                    aquc.Quota_Avail_Flag = true;
                                    aquc.Quota_Dur_Val = Convert.ToByte(q.Quota_Dur_Val);
                                    aquc.Quota_Freq_Val = Convert.ToByte(q.Quota_Freq_Val);
                                    db_transaction.AccountQuotaUsedCurs.Add(aquc);
                                #endregion

                                #region account mobile
                                    AccountMobile am = new AccountMobile();
                                    am.Account = account;
                                    am.Mobile_Number = mobile_no;
                                    am.Primary_Flag = true;
                                    am.Status_Cd = "AC";
                                    am.Updated_By = System.Configuration.ConfigurationManager.AppSettings["CREATED_BY_SMS"];
                                    am.Created_By = System.Configuration.ConfigurationManager.AppSettings["CREATED_BY_SMS"];
                                    db_transaction.AccountMobiles.Add(am);
                                #endregion

                                #region account interest
                                    AccountInterest ai = new AccountInterest();
                                    ai.Account = account;
                                    db_transaction.AccountInterests.Add(ai);
                                #endregion

                                #region account quota
                                    AccountQuota aq = new AccountQuota();
                                    aq.Account = account;
                                    aq.Quota_Cd = q.Quota_Cd;
                                    db_transaction.AccountQuotas.Add(aq);
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

                                    db_transaction.Database.ExecuteSqlCommand(sql_string, no_acct_total, date, output);

                                    int sql_result = Convert.ToInt16(output.Value);

                                    if (sql_result == 0)
                                    { 
                                        account.Status_Cd = FreebieStatus.AccountActivated();
                                        account.Activation_Dttm = timestamp;
                                        reply_str = System.Configuration.ConfigurationManager.AppSettings["ACD"];
                                        sms_log_result = "Register success";
                                        string q_str = (Convert.ToByte(q.Quota_Freq_Val) * Convert.ToByte(q.Quota_Dur_Val) * 30).ToString();
                                        reply_str = reply_str.Replace("{count}", q.Quota_Freq_Val.ToString());
                                        reply_str = reply_str.Replace("{mins}", q.Quota_Dur_Val.ToString());
                                        reply_str = reply_str.Replace("{num}", q_str);
                                    }
                                    else
                                    { 
                                        account.Status_Cd = FreebieStatus.AccountPending();
                                        reply_str = System.Configuration.ConfigurationManager.AppSettings["AP"];
                                        sms_log_result = "Register Pending";
                                    }

                                #endregion


                                account.First_Quota_Cd = q.Quota_Cd;
                                account.First_Quota_Dur_Val = q.Quota_Dur_Val;
                                account.First_Quota_Freq_Val = q.Quota_Freq_Val;
                                account.Dummy_Flag = "0";

                                db_transaction.Accounts.Add(account);
                                db_transaction.SaveChanges();
                                scope.Complete();
                            }
                            #endregion
                            #region call_sp
                                result_sp = CallSP.SP_Insert_Interact_Profile(account.Account_Id);
                                if (!result_sp[0].Equals("0"))
                                {
                                    using (var new_db = new EchoContext())
                                    {
                                        SqlParameter date = new SqlParameter("today", SqlDbType.Date);
                                        date.Value = DateTime.Now;
                                        Account remove_ac = new_db.Accounts.SingleOrDefault(x => x.Account_Id == account.Account_Id);
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
                                            AccountQuotaUsedCur remove_aquc = new_db.AccountQuotaUsedCurs.SingleOrDefault(x => x.Account_Id == account.Account_Id);
                                            if (remove_aquc != null)
                                            {
                                                new_db.AccountQuotaUsedCurs.Remove(remove_aquc);
                                            }
                                            new_db.Accounts.Remove(remove_ac);
                                            new_db.SaveChanges();
                                        }
                                    }
                                    reply_str = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                                }
                                else
                                {
                                    FreebieEvent.AccountCreateEvent(account, account.First_Mobile_Number, Permission.f_cust_regis_page_id);
                                }
                            #endregion
                    }
                    else
                    {
                        reply_str = System.Configuration.ConfigurationManager.AppSettings["WRONG_FORMAT"];
                        sms_log_result = "Wrong input Format";
                    }
                }
            }
            catch (Exception err)
            {
                reply_str = System.Configuration.ConfigurationManager.AppSettings["NO_ACCTACTIVATION"];
                sms_log_result = "System Error";
                FreebieEvent.AddCustomError(err.Message, Permission.f_cust_regis_page_id);
            }

            Encoding encoding = Encoding.GetEncoding("tis-620");
            string xml_str = GetReplyXML(reply_str, encoding);

            sms_log.Result = sms_log_result;
            db.SmsRegistrationLogs.Add(sms_log);
            db.SaveChanges();

            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = encoding;
            context.Response.Write(xml_str);
        }

        public string GetReplyXML(string reply_str, Encoding encoding)
        {
            XmlWriterSettings xml_writing_settings = new XmlWriterSettings();
            xml_writing_settings.Encoding = encoding;
            MemoryStream ms = new MemoryStream();
            using (XmlWriter xml_writer = XmlWriter.Create(ms, xml_writing_settings))
            {
                xml_writer.WriteStartElement("response");
                xml_writer.WriteElementString("status", "success");
                xml_writer.WriteStartElement("data");
                xml_writer.WriteStartElement("record");
                xml_writer.WriteElementString("reply", reply_str);
                xml_writer.WriteEndElement();
                xml_writer.WriteEndElement();
                xml_writer.WriteEndElement();
            }

            return encoding.GetString(ms.ToArray());
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public bool IsValid(string keyword, string content)
        {
            bool result = true;
            string[] gender_validates = new string[] { "M", "F" };
            int min_year = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SMS_MIN_YEAR"]);
            var today = DateTime.Now;
            CultureInfo us = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            today = new DateTime(today.Year, today.Month, today.Day, us.Calendar);
            int max_year = today.Year + 543;

            int[] month30d = new int[] { 4, 6, 9, 11 };
            int[] month31d = new int[] { 1, 3, 5, 7, 8, 10, 12 };
           
            // check keyword
            if (!keyword.ToLower().Equals("regis"))
            {
                return false;
            }

            // check content
            if (content.Length != 10)
            {
                return false;
            }

            string[] content_arrs = content.Split(' ');
            if (content_arrs.Length != 2)
            {
                return false;
            }

            string gender = content_arrs[0];
            string dob = content_arrs[1];

            if (!gender_validates.Contains(gender.ToUpper()))
            {
                return false;
            }

            if (dob.Length != 8)
            {
                return false;
            }
            string day_format = @"(\d\d)";
            string month_format = @"(\d\d)";
            string year_format = @"(\d\d\d\d)";
            string day_str = dob.Substring(0, 2);
            string month_str = dob.Substring(2, 2);
            string year_str = dob.Substring(4, 4);

            if (!System.Text.RegularExpressions.Regex.IsMatch(day_str, day_format))
            {
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(month_str, month_format))
            {
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(year_str, year_format))
            {
                return false;
            }
            byte day = Convert.ToByte(day_str);
            byte month = Convert.ToByte(month_str);
            int year = Convert.ToInt16(year_str);
            

            if (year < min_year || year > max_year)
            {
                return false;
            }

            if (month > 12 || month < 1)
            {
                return false;
            }

            if (day > 31 || day < 1)
            {
                return false;
            }

            if (month30d.Contains(month))
            {
                if (day > 30)
                {
                    return false;
                }
            }
            else
            {
                if (month == 2)
                {
                    if (day > 29)
                    {
                        return false;
                    }
                    else
                    {
                        if (!(year % 400 == 0 || (year % 100 != 0 && year % 4 == 0)))
                        {
                            if (day == 29)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}