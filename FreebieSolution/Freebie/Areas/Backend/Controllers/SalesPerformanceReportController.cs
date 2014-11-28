using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Models;
using Freebie.ViewModels;
using System.Globalization;
using Freebie.Libs;

namespace Freebie.Areas.Backend.Controllers
{
    public class SalesPerformanceReportController : Controller
    {
        //
        // GET: /Backend/SalesPerformanceReport/
        List<SelectListItem> groups = new List<SelectListItem>();
        List<SelectListItem> depts = new List<SelectListItem>();
        List<SelectListItem> users = new List<SelectListItem>();

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult Index()
        {
            init_dropdwon("", 0, "");

            CultureInfo us = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            //CultureInfo th = System.Globalization.CultureInfo.GetCultureInfo("th-TH");

            var today = DateTime.Now;
            today = new DateTime(today.Year, today.Month, today.Day, us.Calendar);
            var start_month = new DateTime(today.Year, today.Month, 1, us.Calendar);
            var set_today = today.ToString("dd/MM/yyyy", us);
            var set_start_month = start_month.ToString("dd/MM/yyyy", us);
            ViewBag.start_date = set_start_month;
            ViewBag.end_date = set_today;
            return View();
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult SalesPerfReportResult()
        {
            string dept = Request.Form["Dept_cd"];
            string current_user_no = Session["User_No"].ToString();

            var db = new EchoContext();
            User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(current_user_no));

            string role_cd = Session["Role"].ToString();

            string start_date_rq = Request.Form["start_date"];
            string end_date_rq = Request.Form["end_date"];

            var start_date = DateTime.Now; 
            var end_date = DateTime.Now;

            try
            {
                if (!string.IsNullOrEmpty(start_date_rq))
                {
                    start_date = DateTime.ParseExact(start_date_rq, @"d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(end_date_rq))
                {
                    end_date = DateTime.ParseExact(end_date_rq, @"d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                start_date = DateTime.Now;
                end_date = DateTime.Now;
            }
            
            
            ViewBag.start_date = String.Format("{0:dd/MM/yyyy}", start_date);
            ViewBag.end_date = String.Format("{0:dd/MM/yyyy}", end_date);
            
            if (string.IsNullOrEmpty(dept))
            {
                dept = "AA";
            }

            string user_no = Request.Form["User_No"];
            string where_str = "";

            if (dept.Equals("0"))
            {
                where_str = String.Format("where u.Dept_Cd != '{0}'", dept);
            }
            else
            {
                where_str = String.Format("where u.Dept_Cd = '{0}' ", dept);
            }

           


            int group_id = Convert.ToInt16(Request.Form["Group_id"]);

            if (group_id != 0)
            {
                where_str += String.Format(" and u.Group_Id = '{0}' ", group_id);
            }

            if (start_date != null && end_date != null)
            {
                where_str += String.Format(" and CAST(a.Registration_Dttm as DATE) between '{0}' and '{1}' ", String.Format("{0:yyyy/MM/dd}", start_date), String.Format("{0:yyyy/MM/dd}", end_date));
            }

            if (!user_no.Equals("0"))
            {
                where_str += String.Format(" and u.User_No = '{0}' ", user_no);
            }
            else
            {
                if (role_cd.Equals("ST"))
                {
                    where_str += String.Format(" and u.User_Id = '{0}' ", current_user.User_Id);
                }
            }
            string sql_str = @"
                select 
	                u.User_No as user_no,
	                CAST(a.Registration_Dttm as DATE) as reg_date,
	                sum(case when a.Status_Cd = 'AC' and a.First_Quota_Cd = 'Q0001'
		                then 1
		                else 0
	                end) as active_low,
	                sum(case when a.Status_Cd = 'AC' and a.First_Quota_Cd = 'Q0002'
		                then 1
		                else 0
	                end) as active_mid,
	                sum(case when a.Status_Cd = 'AC' and a.First_Quota_Cd = 'Q0003'
		                then 1
		                else 0
	                end) as active_high,
	                sum(case when a.Status_Cd in ('AP','ACD') and a.First_Quota_Cd = 'Q0001'
		                then 1
		                else 0
	                end) as pending_low,
	                sum(case when a.Status_Cd in ('AP','ACD') and a.First_Quota_Cd = 'Q0002'
		                then 1
		                else 0
	                end) as pending_mid,
	                sum(case when a.Status_Cd in ('AP','ACD') and a.First_Quota_Cd = 'Q0003'
		                then 1
		                else 0
	                end) as pending_high,
	                sum(case when a.Status_Cd in ('AP','ACD', 'AC') and a.First_Quota_Cd in ('Q0001', 'Q0002', 'Q0003')
		                then 1
		                else 0
	                end)	as user_total
                from [User] as u
                Left join Account a on a.Staff_No = u.User_No
                ";
            sql_str += where_str;

            sql_str += @"
                group by u.User_No, CAST(a.Registration_Dttm as DATE)
                order by u.User_No, CAST(a.Registration_Dttm as DATE)

            ";
            
            

            var results = db.Database.SqlQuery<SalesPerf>(sql_str).ToList();
            init_dropdwon(dept, group_id, user_no);
            return View(results);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public void init_dropdwon(string select_dept, int select_group, string user_no)
        {
            
            string role_cd = Session["Role"].ToString();
            string current_user_no = Session["User_No"].ToString();

            var db = new EchoContext();
            User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(current_user_no));

            

            var all_item = new SelectListItem()
            {
                Text = "ทั้งหมด",
                Value = "0",
                Selected = false
            };

            IEnumerable<Dept> all_depts = db.Depts.OrderBy(s => s.Dept_Cd);

            if (role_cd.Equals("AM"))
            {
                depts.Add(all_item);
            }
           

            if (!role_cd.Equals("AM"))
            {
                all_depts = all_depts.Where(x => x.Dept_Cd.Equals(current_user.Dept_Cd));
            }

            foreach (var d in all_depts)
            {
                var dept = new SelectListItem()
                {
                    Text = d.Dept_Name_En,
                    Value = d.Dept_Cd,
                    Selected = (d.Dept_Cd.Equals(select_dept))
                };
                depts.Add(dept);
            }

            var depts_selectable = depts;
            ViewBag.Dept_Cd = depts_selectable;

            if (!role_cd.Equals("ST"))
            {
                users.Add(all_item);
            }

           
            if (role_cd.Equals("AM"))
            {
                groups.Add(all_item);
            } 
            IEnumerable<User> free_users = db.Users.OrderBy(x => x.User_No);

            if (role_cd.Equals("SU"))
            {
                free_users = free_users.Where(x => x.Dept_Cd.Equals(current_user.Dept_Cd)).Where(x => x.Group_Id == current_user.Group_Id);
            }
            else
            {
                if (role_cd.Equals("ST"))
                {
                    free_users = free_users.Where(x => x.User_Id == current_user.User_Id);
                }
            }
            foreach(var u in free_users)
            {
                var user = new SelectListItem()
                {
                    Text = u.User_No,
                    Value = u.User_No,
                    Selected = (u.User_No == user_no)
                };               
                users.Add(user);
            }
            ViewBag.User_No = users;
            
            for (int i = 1; i <= 20; i++)
            {
                var item = new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString(),
                    Selected = (select_group == i)
                };
                if (role_cd.Equals("AM"))
                {
                    groups.Add(item);
                }
                else
                {
                    if ((role_cd.Equals("SU") || role_cd.Equals("ST")) && i == current_user.Group_Id)
                    {
                        groups.Add(item);
                    }
                }
            }
            ViewBag.Group_Id = groups;

        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ChainSelect()
        {
            int group_id = Convert.ToInt16(Request.Form["Group_Id"]);
            string dept_cd = Request.Form["Dept_Cd"];


            return View("Index");
        }
        public ActionResult CreateTestData()
        {

            //CreateTestUser("AA", 1, null);
            //CreateTestUser("AA", 1, null);
            //CreateTestUser("AA", 2, null);

            //CreateTestUser("AB", 1, null);
            //CreateTestUser("AB", 2, null);
            //CreateTestUser("AB", 2, null);
            //CreateTestUser("AB", 3, null);

            //CreateTestUser("CS", 4, null);

            var d1 = DateTime.Now;
            var d2 = d1.AddDays(-1);
            var d3 = d1.AddDays(-2);

            //CreateTestAccount("S0011", "Q0001", "AC", d1, 1);
            //CreateTestAccount("S0011", "Q0002", "AC", d1, 1);
            //CreateTestAccount("S0011", "Q0003", "AC", d1, 1);
            //CreateTestAccount("S0011", "Q0001", "ACD", d1, 1);
            //CreateTestAccount("S0011", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0011", "Q0003", "ACD", d1, 0);
            //CreateTestAccount("S0011", "Q0001", "AC", d2, 3);
            //CreateTestAccount("S0011", "Q0002", "AC", d2, 1);
            //CreateTestAccount("S0011", "Q0003", "AC", d2, 2);
            //CreateTestAccount("S0011", "Q0001", "ACD", d2, 0);
            //CreateTestAccount("S0011", "Q0002", "ACD", d2, 0);
            //CreateTestAccount("S0011", "Q0003", "ACD", d2, 1);
            //CreateTestAccount("S0011", "Q0001", "AC", d3, 3);
            //CreateTestAccount("S0011", "Q0002", "AC", d3, 2);
            //CreateTestAccount("S0011", "Q0003", "AC", d3, 4);
            //CreateTestAccount("S0011", "Q0001", "ACD", d3, 1);
            //CreateTestAccount("S0011", "Q0002", "ACD", d3, 0);
            //CreateTestAccount("S0011", "Q0003", "ACD", d3, 1);

            //CreateTestAccount("S0012", "Q0001", "AC", d1, 0);
            //CreateTestAccount("S0012", "Q0002", "AC", d1, 0);
            //CreateTestAccount("S0012", "Q0003", "AC", d1, 1);
            //CreateTestAccount("S0012", "Q0001", "ACD", d1, 0);
            //CreateTestAccount("S0012", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0012", "Q0003", "ACD", d1, 0);
            //CreateTestAccount("S0012", "Q0001", "AC", d2, 2);
            //CreateTestAccount("S0012", "Q0002", "AC", d2, 1);
            //CreateTestAccount("S0012", "Q0003", "AC", d2, 2);
            //CreateTestAccount("S0012", "Q0001", "ACD", d2, 4);
            //CreateTestAccount("S0012", "Q0002", "ACD", d2, 0);
            //CreateTestAccount("S0012", "Q0003", "ACD", d2, 0);
            //CreateTestAccount("S0012", "Q0001", "AC", d3, 0);
            //CreateTestAccount("S0012", "Q0002", "AC", d3, 6);
            //CreateTestAccount("S0012", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0012", "Q0001", "ACD", d3, 0);
            //CreateTestAccount("S0012", "Q0002", "ACD", d3, 2);
            //CreateTestAccount("S0012", "Q0003", "ACD", d3, 2);

            //CreateTestAccount("S0013", "Q0001", "AC", d1, 7);
            //CreateTestAccount("S0013", "Q0002", "AC", d1, 0);
            //CreateTestAccount("S0013", "Q0003", "AC", d1, 4);
            //CreateTestAccount("S0013", "Q0001", "ACD", d1, 3);
            //CreateTestAccount("S0013", "Q0002", "ACD", d1, 2);
            //CreateTestAccount("S0013", "Q0003", "ACD", d1, 1);
            //CreateTestAccount("S0013", "Q0001", "AC", d2, 6);
            //CreateTestAccount("S0013", "Q0002", "AC", d2, 8);
            //CreateTestAccount("S0013", "Q0003", "AC", d2, 9);
            //CreateTestAccount("S0013", "Q0001", "ACD", d2, 2);
            //CreateTestAccount("S0013", "Q0002", "ACD", d2, 1);
            //CreateTestAccount("S0013", "Q0003", "ACD", d2, 3);
            //CreateTestAccount("S0013", "Q0001", "AC", d3, 4);
            //CreateTestAccount("S0013", "Q0002", "AC", d3, 1);
            //CreateTestAccount("S0013", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0013", "Q0001", "ACD", d3, 0);
            //CreateTestAccount("S0013", "Q0002", "ACD", d3, 0);
            //CreateTestAccount("S0013", "Q0003", "ACD", d3, 0);

            //CreateTestAccount("S0014", "Q0001", "AC", d1, 4);
            //CreateTestAccount("S0014", "Q0002", "AC", d1, 3);
            //CreateTestAccount("S0014", "Q0003", "AC", d1, 2);
            //CreateTestAccount("S0014", "Q0001", "ACD", d1, 1);
            //CreateTestAccount("S0014", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0014", "Q0003", "ACD", d1, 0);
            //CreateTestAccount("S0014", "Q0001", "AC", d2, 0);
            //CreateTestAccount("S0014", "Q0002", "AC", d2, 0);
            //CreateTestAccount("S0014", "Q0003", "AC", d2, 1);
            //CreateTestAccount("S0014", "Q0001", "ACD", d2, 2);
            //CreateTestAccount("S0014", "Q0002", "ACD", d2, 0);
            //CreateTestAccount("S0014", "Q0003", "ACD", d2, 3);
            //CreateTestAccount("S0014", "Q0001", "AC", d3, 1);
            //CreateTestAccount("S0014", "Q0002", "AC", d3, 2);
            //CreateTestAccount("S0014", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0014", "Q0001", "ACD", d3, 3);
            //CreateTestAccount("S0014", "Q0002", "ACD", d3, 0);
            //CreateTestAccount("S0014", "Q0003", "ACD", d3, 3);

            //CreateTestAccount("S0015", "Q0001", "AC", d1, 5);
            //CreateTestAccount("S0015", "Q0002", "AC", d1, 0);
            //CreateTestAccount("S0015", "Q0003", "AC", d1, 5);
            //CreateTestAccount("S0015", "Q0001", "ACD", d1, 2);
            //CreateTestAccount("S0015", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0015", "Q0003", "ACD", d1, 1);
            //CreateTestAccount("S0015", "Q0001", "AC", d2, 1);
            //CreateTestAccount("S0015", "Q0002", "AC", d2, 1);
            //CreateTestAccount("S0015", "Q0003", "AC", d2, 1);
            //CreateTestAccount("S0015", "Q0001", "ACD", d2, 1);
            //CreateTestAccount("S0015", "Q0002", "ACD", d2, 0);
            //CreateTestAccount("S0015", "Q0003", "ACD", d2, 1);
            //CreateTestAccount("S0015", "Q0001", "AC", d3, 0);
            //CreateTestAccount("S0015", "Q0002", "AC", d3, 0);
            //CreateTestAccount("S0015", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0015", "Q0001", "ACD", d3, 6);
            //CreateTestAccount("S0015", "Q0002", "ACD", d3, 1);
            //CreateTestAccount("S0015", "Q0003", "ACD", d3, 2);

            //CreateTestAccount("S0016", "Q0001", "AC", d1, 4);
            //CreateTestAccount("S0016", "Q0002", "AC", d1, 1);
            //CreateTestAccount("S0016", "Q0003", "AC", d1, 1);
            //CreateTestAccount("S0016", "Q0001", "ACD", d1, 1);
            //CreateTestAccount("S0016", "Q0002", "ACD", d1, 1);
            //CreateTestAccount("S0016", "Q0003", "ACD", d1, 2);
            //CreateTestAccount("S0016", "Q0001", "AC", d2, 1);
            //CreateTestAccount("S0016", "Q0002", "AC", d2, 0);
            //CreateTestAccount("S0016", "Q0003", "AC", d2, 0);
            //CreateTestAccount("S0016", "Q0001", "ACD", d2, 0);
            //CreateTestAccount("S0016", "Q0002", "ACD", d2, 3);
            //CreateTestAccount("S0016", "Q0003", "ACD", d2, 4);
            //CreateTestAccount("S0016", "Q0001", "AC", d3, 4);
            //CreateTestAccount("S0016", "Q0002", "AC", d3, 2);
            //CreateTestAccount("S0016", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0016", "Q0001", "ACD", d3, 3);
            //CreateTestAccount("S0016", "Q0002", "ACD", d3, 2);
            //CreateTestAccount("S0016", "Q0003", "ACD", d3, 1);

            //CreateTestAccount("S0017", "Q0001", "AC", d1, 7);
            //CreateTestAccount("S0017", "Q0002", "AC", d1, 0);
            //CreateTestAccount("S0017", "Q0003", "AC", d1, 9);
            //CreateTestAccount("S0017", "Q0001", "ACD", d1, 0);
            //CreateTestAccount("S0017", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0017", "Q0003", "ACD", d1, 1);
            //CreateTestAccount("S0017", "Q0001", "AC", d2, 0);
            //CreateTestAccount("S0017", "Q0002", "AC", d2, 1);
            //CreateTestAccount("S0017", "Q0003", "AC", d2, 2);
            //CreateTestAccount("S0017", "Q0001", "ACD", d2, 3);
            //CreateTestAccount("S0017", "Q0002", "ACD", d2, 4);
            //CreateTestAccount("S0017", "Q0003", "ACD", d2, 5);
            //CreateTestAccount("S0017", "Q0001", "AC", d3, 4);
            //CreateTestAccount("S0017", "Q0002", "AC", d3, 1);
            //CreateTestAccount("S0017", "Q0003", "AC", d3, 2);
            //CreateTestAccount("S0017", "Q0001", "ACD", d3, 4);
            //CreateTestAccount("S0017", "Q0002", "ACD", d3, 3);
            //CreateTestAccount("S0017", "Q0003", "ACD", d3, 2);

            //CreateTestAccount("S0018", "Q0001", "AC", d1, 1);
            //CreateTestAccount("S0018", "Q0002", "AC", d1, 2);
            //CreateTestAccount("S0018", "Q0003", "AC", d1, 3);
            //CreateTestAccount("S0018", "Q0001", "ACD", d1, 4);
            //CreateTestAccount("S0018", "Q0002", "ACD", d1, 0);
            //CreateTestAccount("S0018", "Q0003", "ACD", d1, 1);
            //CreateTestAccount("S0018", "Q0001", "AC", d2, 1);
            //CreateTestAccount("S0018", "Q0002", "AC", d2, 0);
            //CreateTestAccount("S0018", "Q0003", "AC", d2, 5);
            //CreateTestAccount("S0018", "Q0001", "ACD", d2, 4);
            //CreateTestAccount("S0018", "Q0002", "ACD", d2, 1);
            //CreateTestAccount("S0018", "Q0003", "ACD", d2, 2);
            //CreateTestAccount("S0018", "Q0001", "AC", d3, 0);
            //CreateTestAccount("S0018", "Q0002", "AC", d3, 1);
            //CreateTestAccount("S0018", "Q0003", "AC", d3, 1);
            //CreateTestAccount("S0018", "Q0001", "ACD", d3, 1);
            //CreateTestAccount("S0018", "Q0002", "ACD", d3, 7);
            //CreateTestAccount("S0018", "Q0003", "ACD", d3, 1);
            return View();
        }

        //public void CreateTestUser(string dept_cd, int group_id, string role)
        //{
        //    var db = new EchoContext();

        //    User u = new User();
        //    string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    string username = "";
        //    Random rnd = new Random();
        //    for (int i = 0; i <= 5; i++)
        //    {
        //        int iRandom = rnd.Next(0, strPwdchar.Length - 1);
        //        username += strPwdchar.Substring(iRandom, 1);
        //    }

        //    u.User_Name = username;
        //    u.Status_Cd = "AC";
        //    u.Dept_Cd = dept_cd;
        //    u.Group_Id = Convert.ToByte(group_id);
        //    u.Role_Cd = "ST";

        //    db.Users.Add(u);
        //    db.SaveChanges();
        //}

        //public void CreateTestAccount(string user_no, string quota_cd, string status_cd, DateTime create_date, int count)
        //{
        //    if (count > 0)
        //    {
        //        var db = new EchoContext();

        //        for (int i = 1; i <= count; i++)
        //        {
        //            Account account = new Account();
        //            account.First_Name = "test_report";
        //            account.Last_Name = "test report";
        //            account.Status_Cd = status_cd;
        //            account.Day_Of_Birth = 1;
        //            account.Month_Of_Birth = 1;
        //            account.Year_Of_Birth = 1980;
        //            account.Gender_Cd = "M";
        //            account.First_Quota_Cd = quota_cd;
        //            account.Staff_No = user_no;
        //            account.Registration_Dttm = create_date;
        //            account.Created_Dttm = create_date;

        //            db.Accounts.Add(account);
        //        }
        //        db.SaveChanges();
            
        //    }
            
        //}
    }
}
