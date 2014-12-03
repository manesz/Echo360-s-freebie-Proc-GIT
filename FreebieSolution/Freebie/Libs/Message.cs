using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.IO;
using Freebie.Models;

namespace Freebie.Libs
{
    public static class Message
    {
        public static void SendUsername(string phone_number)
        {
            using (var db = new EchoContext())
            {
                AccountMobile am = db.AccountMobiles.Where(x => x.Mobile_Number.Equals(phone_number)).Where(x => x.Status_Cd.Equals("AC")).SingleOrDefault();
                string username = "";

                if (am != null)
                {
                    Account ac = db.Accounts.SingleOrDefault(x => x.Account_Id == am.Account_Id);
                    if (ac != null)
                    {
                        username = ac.User_Name;
                        string message = System.Configuration.ConfigurationManager.AppSettings["SMS_MESSAGE_FORGOT_USERNAME"];
                        message = message.Replace("{username}", username);
                        OtpLog log = new OtpLog();
                        string result = fire(phone_number, message, log);
                    }
                }               
            }           
        }

        public static void SendOTPReg(string phone_number, string otp)//ORIGIN
        //public static string SendOTPReg(string phone_number, string otp)//DEBUG
        {
            string message = System.Configuration.ConfigurationManager.AppSettings["SMS_MESSAGE_OTP_REG"];
            string time_exp = System.Configuration.ConfigurationManager.AppSettings["OTPPwdExpired"];
            message = message.Replace("{otp}", otp);
            message = message.Replace("{time}", time_exp);
            OtpLog log = new OtpLog();
            string result = fire(phone_number, message, log); //ORIGIN
            //return result;//DEBUG
        }

        public static void SendOTPUsername(string phone_number, string otp)
        {
            string message = System.Configuration.ConfigurationManager.AppSettings["SMS_MESSAGE_OTP_USERNAME"];
            string time_exp = System.Configuration.ConfigurationManager.AppSettings["OTPPwdExpired"];
            message = message.Replace("{otp}", otp);
            message = message.Replace("{time}", time_exp);
            OtpLog log = new OtpLog();
            string result = fire(phone_number, message, log);
        }

        public static string Notify_Account(string phone_number)
        {
            string message = System.Configuration.ConfigurationManager.AppSettings["ACTIVE_ACCOUNT_SMS"];
            ActivationSmsLog log = new ActivationSmsLog();
            string result = fire(phone_number, message, log);
            return result;
        }

        private static void Callback(HttpWebRequest request, Action<HttpWebResponse> responseAction)
        {
            Action wrapperAction = () =>
            {
                request.BeginGetResponse(new AsyncCallback((iar) =>
                {
                    var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                    responseAction(response);
                }), request);
            };
            wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
            {
                var action = (Action)iar.AsyncState;
                action.EndInvoke(iar);
            }), wrapperAction);
        }
        private static string fire(string phone_number, string msg, object log)
        {
            string result = string.Empty;
           
            System.Type type_of = log.GetType();//ORIGIN

            System.Configuration.ConfigurationManager.AppSettings["SEND_SMS"] = "YES";//dummy data for DEBUG

            if (System.Configuration.ConfigurationManager.AppSettings["SEND_SMS"].Equals("YES"))
            {

                string postData = "ACCOUNT=" + System.Configuration.ConfigurationManager.AppSettings["MOBILE_ACCOUNT"];
                postData += "&PASSWORD=" + System.Configuration.ConfigurationManager.AppSettings["MOBILE_PWD"];
                postData += "&MOBILE=" + phone_number;

                postData += "&MESSAGE=" + msg;

                postData += "&LANGUAGE=" + System.Configuration.ConfigurationManager.AppSettings["MESSAGE_LANGUAGE"];
                postData += "&SENDER=" + System.Configuration.ConfigurationManager.AppSettings["SENDER_NAME"];
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                Encoding iso = Encoding.GetEncoding("ISO-8859-11");
                Encoding utf8 = Encoding.UTF8;
                byte[] data = encoding.GetBytes(postData);
                data = Encoding.Convert(utf8, iso, data);

                DateTime start_res = DateTime.Now;

                //result = "System.Configuration.ConfigurationManager.AppSettings['SEND_SMS'].Equals('Yes')"; //DEBUG

                try
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["SMS_GATEWAY"]);
                    httpRequest.Method = "POST";
                    httpRequest.Host = System.Configuration.ConfigurationManager.AppSettings["SMS_HOST"]; //"203.146.102.26";
                    httpRequest.ContentType = "application/x-www-form-urlencoded";
                    httpRequest.ContentLength = data.Length;
                    MemoryStream Memstream = new MemoryStream(data);
                    Stream stream = httpRequest.GetRequestStream();
                    Memstream.WriteTo(stream);
                    stream.Close();

                    Callback(httpRequest, (response) =>
                    {
                        var res_stream = new StreamReader(response.GetResponseStream());
                        DateTime end_res = DateTime.Now;
                        int index = 0;
                        string[] result_rsp = new string[] { "", "", "", "" };
                        string rsp_str = "";
                        while (res_stream.Peek() >= 0)
                        {
                            result_rsp[index] = res_stream.ReadLine();
                            index++;
                        }
                        foreach (var txt in result_rsp)
                        {
                            rsp_str += txt + " ";
                        }

                        //saving
                        using (var db = new EchoContext())
                        {
                            if (type_of == typeof(OtpLog))
                            {
                                OtpLog logger = new OtpLog();
                                logger.Mobile_Number = phone_number;
                                logger.Request_At = start_res;
                                logger.Response_At = end_res;
                                logger.Response_Text = rsp_str;
                                db.OtpLogs.Add(logger);
                                db.SaveChanges();
                            }
                            else
                            {
                                if (type_of == typeof(ActivationSmsLog))
                                {
                                    ActivationSmsLog logger = new ActivationSmsLog();
                                    logger.Mobile_Number = phone_number;
                                    logger.Request_At = start_res;
                                    logger.Response_At = end_res;
                                    logger.Response_Text = rsp_str;
                                    db.ActivationSmsLogs.Add(logger);
                                    db.SaveChanges();
                                }
                            }
                        }
                        res_stream.Close();
                    });
                }
                catch (WebException wex)
                {
                    using (var db = new EchoContext())
                    {
                        if (type_of == typeof(OtpLog))
                        {
                            OtpLog logger = new OtpLog();
                            logger.Mobile_Number = phone_number;
                            logger.Request_At = start_res;
                            logger.Response_At = DateTime.Now;
                            logger.Response_Text = wex.Message;
                            db.OtpLogs.Add(logger);
                            db.SaveChanges();
                        }
                        else
                        {
                            if (type_of == typeof(ActivationSmsLog))
                            {
                                ActivationSmsLog logger = new ActivationSmsLog();
                                logger.Mobile_Number = phone_number;
                                logger.Request_At = start_res;
                                logger.Response_At = DateTime.Now;
                                logger.Response_Text = wex.Message;
                                db.ActivationSmsLogs.Add(logger);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FreebieEvent.AddError(ex, 0);
                }
            }
            else {
                result = ".AppSettings['SEND_SMS'].Equals('NO')"; //DEBUG
            }

            return result;//ORIGIN
        }
    }
}