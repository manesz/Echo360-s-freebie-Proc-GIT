using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using Base32;
using Freebie.Models;
using System.Data;
using System.Net;
using System.IO;

namespace Freebie.Libs
{
    public static class OTPHandler
    {
        public static string RequestOTP(string phone_number)
        {
            var today = DateTime.Now;
            int limit_daily = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["OTP_ALLOW_PER_DAY_PER_NUMBER"]);
            int interval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["INTERVAL_PERIOD_BETWEEN_OTP"]);

            using(var db = new EchoContext()) {
                phone_number = phone_number.Replace("-", "");
                long number = Convert.ToInt64(phone_number);

                OTPRequest otp_request = db.OTPRequests.Where(x => x.Date.Equals(today.Date)).Where(x => x.PhoneNumber.Equals(phone_number)).SingleOrDefault();
                var request_time = DateTime.Now;
                if (otp_request == null)
                {
                    bool flag = true;
                    otp_request = new OTPRequest();
                    otp_request.PhoneNumber = phone_number;
                    otp_request.Count = 0;

                    OTP otp = db.OTPs.SingleOrDefault(x => x.PhoneNumber.Equals(phone_number));
                    string secret_str = Secret();
                    if (otp == null)
                    {
                        otp = new OTP();
                        flag = false;
                    }
                    
                    otp.Secret = secret_str;
                    otp.PhoneNumber = phone_number;
                    otp.Counter = 0;
                    otp.Expired_Dttm = request_time.AddMinutes(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["OTPPwdExpired"]));

                    if (flag)
                    { db.Entry(otp).State = EntityState.Modified; }
                    else
                    { db.OTPs.Add(otp); }

                    otp_request.Last_Request_At = request_time;
                    otp_request.PhoneNumber = phone_number;
                    otp_request.Date = request_time.Date;
                    otp_request.Count = 1;

                    db.OTPRequests.Add(otp_request);

                    db.SaveChanges();
                    return GenerateOTP(secret_str, number);
                }
                else
                {
                    TimeSpan diff = request_time.Subtract(Convert.ToDateTime(otp_request.Last_Request_At));
                    if (diff.TotalMinutes > interval && otp_request.Count < limit_daily)
                    {
                        bool flag = true;
                        OTP otp = db.OTPs.SingleOrDefault(x => x.PhoneNumber.Equals(phone_number));
                        string secret_str = Secret();
                        if (otp == null)
                        {
                            otp = new OTP();
                            flag = false;
                        }
                        otp.Secret = secret_str;
                        otp.PhoneNumber = phone_number;
                        otp.Counter = 0;
                        otp.Expired_Dttm = request_time.AddMinutes(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["OTPPwdExpired"]));

                        if (flag)
                        { db.Entry(otp).State = EntityState.Modified; }
                        else
                        { db.OTPs.Add(otp); }

                        otp_request.Last_Request_At = request_time;
                        otp_request.Count += 1;

                        db.Entry(otp_request).State = EntityState.Modified;

                        db.SaveChanges();
                        return GenerateOTP(secret_str, number);
                    }
                    else {
                        if (otp_request.Count >= limit_daily)
                        {
                            return "limit_daily";
                        }

                        return "limit_interval";
                        
                    }
                }
            }
        }

        public static int ValidateOTP(string phone_number, string otp_pwd)
        {
            /* returning value
             *  0 - pass
             *  1 - failed, try again
             *  2 - failed, start over
             *  3 - expired, start over
            */
            long number = Convert.ToInt64(phone_number);

            using (var db = new EchoContext())
            {
                OTP otp = db.OTPs.SingleOrDefault(x => x.PhoneNumber.Equals(phone_number));

                if (otp == null) { return 2; }
                if (otp.Counter >= 3) { return 2; }
                int cmp = DateTime.Compare(DateTime.Now, otp.Expired_Dttm);

                if (cmp > 0) { return 3;}

                string check_otp = GenerateOTP(otp.Secret, number);
                if (check_otp.Equals(otp_pwd))
                {
                    return 0;
                }
                else
                {
                    otp.Counter += 1;
                    db.Entry(otp).State = EntityState.Modified;
                    db.SaveChanges();
                    if (otp.Counter >= 3)
                    { return 2; }
                    return 1;
                }
            }
        }

        public static string SendOTPReg(string phone_number)
        {
            string result = "";

            string otp = RequestOTP(phone_number);
            if (otp.Equals("limit_daily"))
            {
                //return otp;//ORIGIN
                return "SendOTPReg_limit_daily";
            }
            if (otp.Equals("limit_interval"))
            {
                //return otp;//ORIGIN
                return "SendOTPReg_limit_interval";
            }
            Message.SendOTPReg(phone_number, otp);


            //result = Message.SendOTPReg(phone_number, otp);//DEBUG
            //return otp; //return for debugging
            return result; //DEBUG
        }

        public static string SendOTPUsername(string phone_number)
        {
            string otp = RequestOTP(phone_number);
            if (otp.Equals("limit_daily"))
            {
                //return otp;//ORIGIN
                return "SendOTPUsername_limit_daily";
            }
            if (otp.Equals("limit_interval"))
            {
                //return otp;
                return "SendOTPUsername_limit_interval";
            }
            Message.SendOTPUsername(phone_number, otp);
            return otp; //return for debugging
        }


        private static string Secret()
        {
            string secret = "";

            byte[] buffer = new byte[9];
            using (RandomNumberGenerator rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(buffer);
            }

            secret = Convert.ToBase64String(buffer).Substring(0, 10).Replace('/', '0').Replace('+', '1');
            var enc = Base32Encoder.Encode(Encoding.ASCII.GetBytes(secret));
            return enc;
        }

        private static string GenerateOTP(string secret_str, long number)
        {
            int return_digits = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["OTP_DIGITS"]);

            byte[] counter = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);
            byte[] key = Encoding.ASCII.GetBytes(secret_str);
            HMACSHA1 hmac = new HMACSHA1(key, true);
            Byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, return_digits);

            return password.ToString(new string('0', return_digits));
        }
    }
}