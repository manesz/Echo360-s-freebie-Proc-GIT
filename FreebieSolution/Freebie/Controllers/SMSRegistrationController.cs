using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Libs;
using System.Text;
using System.Net;
using System.IO;
using System.Transactions;
using Freebie.Models;

namespace Freebie.Controllers
{
    public class SMSRegistrationController : Controller
    {
        //
        // GET: /Registration/

        public ActionResult Test()
        {
            return View();
        }

        //public ActionResult TestSMS()
        //{
        //    //string response = "";
        //    //for (int i = 1; i <= 500; i++)
        //    //{
        //    //    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        //    //    Encoding iso = Encoding.GetEncoding("ISO-8859-11");
        //    //    Encoding utf8 = Encoding.UTF8;
        //    //    string postData = "mobile_no=" + "088000" + string.Format("{0:0000}", i);
        //    //    postData += "&keyword=regis";
        //    //    postData += "&content=m 24072530";
        //    //    byte[] data = encoding.GetBytes(postData);
        //    //    data = Encoding.Convert(utf8, iso, data);
        //    //    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:1205/SMSRegistration");
        //    //    httpRequest.Method = "POST";
        //    //    httpRequest.ContentType = "application/x-www-form-urlencoded";
        //    //    Stream newStream = httpRequest.GetRequestStream();
        //    //    // Send the data.
        //    //    newStream.Write(data, 0, data.Length);
        //    //    newStream.Close();

        //    //    StreamReader rsp_stream = new StreamReader(httpRequest.GetResponse().GetResponseStream());
        //    //    string resp = rsp_stream.ReadToEnd();
        //    //    response += resp + " ";

        //    //}
        //    //ViewBag.Response = response;
        //    var db = new EchoContext();
            
        //    using (TransactionScope scope = new TransactionScope())
        //    {
                
        //        OTPRequest req = new OTPRequest();
        //        req.PhoneNumber = "0800000000";
        //        req.Date = DateTime.Now.Date;
        //        req.Last_Request_At = DateTime.Now;
        //        req.Count = 0;

        //        db.OTPRequests.Add(req);

        //        db.SaveChanges();
                
        //        scope.Complete();

                
        //    }
        //    return View();
        //}
        [HttpPost]
        public void Index(string state)
        {
            HttpContext context = System.Web.HttpContext.Current;
            Reply rp = new Reply();
            rp.ProcessRequest(context);
        }

        [HttpPost]
        public void Create()
        {
            HttpContext context = System.Web.HttpContext.Current;
            Reply rp = new Reply();
            rp.ProcessRequest(context);
        }

    }
}
