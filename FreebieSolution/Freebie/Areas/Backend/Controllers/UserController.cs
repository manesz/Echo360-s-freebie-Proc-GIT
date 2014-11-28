using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Freebie.Models;
using Freebie.Libs;
using System.Web.Security;
using System.Data;
using PagedList;

namespace Freebie.Areas.Backend.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /Backend/User/
        private EchoContext db = new EchoContext();
        List<SelectListItem> groups = new List<SelectListItem>();

        public ActionResult Index()
        {
            return View();
        }
        
        public class HomeController
        {
            //...
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult StaffProfile()
        {
            string user_no = "";
            if (Session["User_No"] != null)
            {
                user_no = Session["User_No"].ToString();
            }
            User user = db.Users.Where(x => x.User_No.Equals(user_no)).Where(x => x.Status_Cd.Equals("AC")).SingleOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult StaffAcct(string dept_cd, int? group_id, int? page)
        {
            string role_cd = Session["Role"].ToString();
            string user_no = Session["User_No"].ToString();
            User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));

            IEnumerable<User> staffs =  db.Users.Where(x => x.Role_Cd.Equals("ST")).OrderBy(x => x.User_No);

            var pageNumber = page ?? 1;

            if (!role_cd.Equals("AM"))
            {
                if (role_cd.Equals("SU"))
                {
                    staffs = staffs.Where(x => x.Role_Cd.Equals("ST")).Where(x => x.Group_Id == current_user.Group_Id);
                    staffs = staffs.Where(x => x.Dept_Cd.Equals(current_user.Dept_Cd));
                }
                else
                {
                    staffs = staffs.Where(x => x.User_No.Equals(user_no));
                }
            }
            else
            {
                if (group_id != null)
                {
                    group_id = Convert.ToByte(group_id);
                    staffs = staffs.Where(x => x.Group_Id == group_id);
                }

                if (!string.IsNullOrWhiteSpace(dept_cd))
                {
                    staffs = staffs.Where(x => x.Dept_Cd.Equals(dept_cd));
                }
            }
            

            int pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["StaffPerPage"]);
            staffs = staffs.ToList().ToPagedList(pageNumber, pageSize);
            ViewBag.PageStaffs = staffs;
            ViewBag.pageNumber = pageNumber;
            ViewBag.pageSize = pageSize;
            return View(staffs);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult SupervisorAcct(int? page)
        {
            IEnumerable<User> staffs = db.Users.Where(x => x.Role_Cd.Equals("SU")).OrderBy(x => x.User_No);
            var pageNumber = page ?? 1;
            int pageSize =  Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["StaffPerPage"]);
            staffs = staffs.ToList().ToPagedList(pageNumber, pageSize);
            ViewBag.PageStaffs = staffs;
            ViewBag.pageNumber = pageNumber;
            ViewBag.pageSize = pageSize;
            return View(staffs);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ViewStaffAcct(int? user_id)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);
            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (user == null) {
                return HttpNotFound();
            }

            return View(user);
        
        }
        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult ViewSupervisorAcct(int? user_id)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);
            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult CreateStaffAcct()
        {
            User u = new User();
            init_dropdown(u);
            return View(u);
        }
        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult CreateSupervisorAcct()
        {
            User u = new User();
            init_dropdown(u);
            return View(u);
        }
        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateStaffAcct(int? user_id)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);
            User user = db.Users.Where(x => x.User_Id == user_id).SingleOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }

            init_dropdown(user);
            return View(user);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateSupervisorAcct(int? user_id)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);
            User user = db.Users.Where(x => x.User_Id == user_id).SingleOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }

            bool can_crud_this_user = Permission.can_update_this_staff(user);
            if (!can_crud_this_user)
            {
                return HttpNotFound();
            }
            init_dropdown(user);
            return View(user);
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateStaffAcct(int? user_id, string state)
        {
            

            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);

            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(Request.Form["First_Name"]))
            {
                ModelState.AddModelError("First_Name", "กรุณาระบุชื่อ");
            }
            if (string.IsNullOrWhiteSpace(Request.Form["Last_Name"]))
            {
                ModelState.AddModelError("Last_Name", "กรุณาระบุนามสกุล");
            }

            bool can_crud_this_user = Permission.can_update_this_staff(user);
            if (!can_crud_this_user)
            {
                return HttpNotFound();
            }

            user.First_Name = Request.Form["First_Name"];
            user.Last_Name = Request.Form["Last_Name"];
            user.Dept_Cd = Request.Form["Dept_Cd"];
            user.Group_Id = Convert.ToByte(Request.Form["Group_Id"]);
            user.Status_Cd = Request.Form["Status_Cd"];

            if (!string.IsNullOrWhiteSpace(Request.Form["PlainPwd"]))
            {
                string pwd = Request.Form["PlainPwd"];
                user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
            }

            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.staff_acct_page_id, "A04");
                //init_dropdown(user);
                return View("ViewStaffAcct", user);
            }
            else
            {
                init_dropdown(user);
                return View(user);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateSupervisorAcct(int? user_id, string state)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);

            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(Request.Form["First_Name"]))
            {
                ModelState.AddModelError("First_Name", "กรุณาระบุชื่อ");
            }
            if (string.IsNullOrWhiteSpace(Request.Form["Last_Name"]))
            {
                ModelState.AddModelError("Last_Name", "กรุณาระบุนามสกุล");
            }


            bool can_crud_this_user = Permission.can_update_this_staff(user);
            if (!can_crud_this_user)
            {
                return HttpNotFound();
            }

            int group_id = Convert.ToByte(Request.Form["Group_Id"]);
            string dept_cd = Request.Form["Dept_Cd"];
            User check_existing = db.Users.Where(x => x.Dept_Cd.Equals(dept_cd)).Where(x => x.Group_Id == group_id).Where(x => x.Role_Cd.Equals("SU")).SingleOrDefault();

            

            user.First_Name = Request.Form["First_Name"];
            user.Last_Name = Request.Form["Last_Name"];
            user.Dept_Cd = Request.Form["Dept_Cd"];
            user.Group_Id = Convert.ToByte(Request.Form["Group_Id"]);
            user.Status_Cd = Request.Form["Status_Cd"];

            if (!string.IsNullOrWhiteSpace(Request.Form["PlainPwd"]))
            {
                string pwd = Request.Form["PlainPwd"];
                user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
            }
            if (check_existing != null && check_existing.User_Id != user.User_Id)
            {
                ModelState.AddModelError("User_Name", System.Configuration.ConfigurationManager.AppSettings["SU_EXISTS"]);
                init_dropdown(user);
                return View(user);
            }

            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.sup_acct_page_id, "A04");
                return View("ViewSupervisorAcct", user);
            }
            else
            {
                init_dropdown(user);
                return View(user);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult GeneratePassword(int? user_id)
        {
            string user_no = Session["User_No"].ToString();
            string role_cd = Session["Role"].ToString();

            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);

            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (user == null)
            {
                return HttpNotFound();
            }

            bool can_crud_this_user = Permission.can_update_this_staff(user);
            if (!can_crud_this_user)
            {
                return HttpNotFound();
            }

            string pwd = PasswordGenerator.Get();
            ViewBag.PlainPwd = pwd;
            //user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
           // db.Entry(user).State = EntityState.Modified;
            //db.SaveChanges();
            //FreebieEvent.UserUpdateEvent(Permission.staff_profile_page_id, "A04");
            init_dropdown(user);
            if (user.Role_Cd.Equals("ST"))
            {
                
                return View("UpdateStaffAcct", user);
            }
            else
            {
                return View("UpdateSupervisorAcct", user);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult RemoveStaff(int? user_id)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);

            User u = db.Users.SingleOrDefault(x => x.User_Id == user_id);
            if (u == null)
            {
                return HttpNotFound();
            }


            bool can_crud_this_user = Permission.can_update_this_staff(u);
            if (!can_crud_this_user)
            {
                return HttpNotFound();
            }

            bool is_sup = false;
            if (u.Role_Cd.Equals("SU"))
            {
                is_sup = true;
            }

            db.Users.Remove(u);
            db.SaveChanges();
            if (is_sup)
            {
                FreebieEvent.UserUpdateEvent(Permission.sup_acct_page_id, "A05");
                return RedirectToAction("SupervisorAcct");
            }
            FreebieEvent.UserUpdateEvent(Permission.staff_acct_page_id, "A05");
            return RedirectToAction("StaffAcct");
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult CreateStaffAcct(User u)
        {
            if (string.IsNullOrWhiteSpace(u.First_Name))
            {
                ModelState.AddModelError("First_Name", "กรุณาระบุชื่อ");
            }
            if (string.IsNullOrWhiteSpace(u.Last_Name))
            {
                ModelState.AddModelError("Last_Name", "กรุณาระบุนามสกุล");
            }
            if (ModelState.IsValid)
            {
                string pwd = PasswordGenerator.Get();
                u.Role_Cd = "ST";
                u.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
                u.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                u.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                u.Created_Dttm = DateTime.Now;
                u.Updated_Dttm = DateTime.Now;
                db.Users.Add(u);
                db.SaveChanges();
                FreebieEvent.UserCreateEvent(Permission.staff_acct_page_id);
                return RedirectToAction("AssignUserName", new { user_id = u.User_Id, pwd = pwd});
            }
            else
            {
                init_dropdown(u);
                return View(u);
            }
           
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult CreateSupervisorAcct(User u)
        {
            int group_id = Convert.ToInt16(u.Group_Id);
            string dept_cd = u.Dept_Cd;

            if (string.IsNullOrWhiteSpace(u.First_Name))
            {
                ModelState.AddModelError("First_Name", "กรุณาระบุชื่อ");
            }
            if (string.IsNullOrWhiteSpace(u.Last_Name))
            {
                ModelState.AddModelError("Last_Name", "กรุณาระบุนามสกุล");
            }

            User check_existing = db.Users.Where(x => x.Dept_Cd.Equals(dept_cd)).Where(x => x.Group_Id == group_id).Where(x => x.Role_Cd.Equals("SU")).SingleOrDefault();

            if (check_existing != null)
            {
                ModelState.AddModelError("User_Name", System.Configuration.ConfigurationManager.AppSettings["SU_EXISTS"]);
            }
            if (ModelState.IsValid)
            {
                string pwd = PasswordGenerator.Get();
                u.Role_Cd = "SU";
                u.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
                u.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                u.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
                u.Created_Dttm = DateTime.Now;
                u.Updated_Dttm = DateTime.Now;
                db.Users.Add(u);
                db.SaveChanges();
                FreebieEvent.UserCreateEvent(Permission.sup_acct_page_id);
                return RedirectToAction("AssignUserName", new { user_id = u.User_Id, pwd = pwd });
            }
            else
            {
                init_dropdown(u);
                return View(u);
            }

        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult AssignUserName(int? user_id, string pwd)
        {
            if (user_id == null)
            { return HttpNotFound(); }
            user_id = Convert.ToInt32(user_id);

            User user = db.Users.SingleOrDefault(x => x.User_Id == user_id);

            if (user != null)
            {
                bool can_crud_this_user = Permission.can_update_this_staff(user);
                if (!can_crud_this_user)
                {
                    return HttpNotFound();
                }

                user.User_Name = user.User_No;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            ViewBag.PlainPwd = pwd;
            ViewBag.Status = user.Status();
            ViewBag.Dept = user.Dept.Dept_Name_En;
            if (user.Role_Cd.Equals("ST"))
            {
                return View("CreateStaffAcctResult", user);
            }
            else
            {
                return View("CreateSupervisorAcctResult", user);
            }
            
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        public ActionResult UpdateStaffPwd()
        {
            string user_no = Session["User_No"].ToString();

            User user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));

            if (user == null)
            {
                return HttpNotFound();
            }


            return View();
        
        }

        [FreebieAuthorization(Url = "~/Backend/Authentication/Login", Type = "Backend")]
        [HttpPost]
        public ActionResult UpdateStaffPwd(string status)
        {
            string current_pwd = Request.Form["CurrentPassword"];
            string new_pwd = Request.Form["NewPassword"];
            string confirm_pwd = Request.Form["ConfirmNewPassword"];
           

            if (string.IsNullOrEmpty(current_pwd) || string.IsNullOrEmpty(new_pwd) || string.IsNullOrEmpty(confirm_pwd))
            {
                ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["STAFF_PWD"];
                return View();
            }

            if (new_pwd.Length < 6 || new_pwd.Length > 15)
            {
                ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["Validate008"];
                return View();
            }

            if (new_pwd != confirm_pwd)
            {
                ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["Validate006"];
                return View();
            }

            

            var enc = FormsAuthentication.HashPasswordForStoringInConfigFile(current_pwd, "SHA1");

            string user_no = Session["User_No"].ToString();

            User user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));

            if (user != null)
            {
                if (!user.Password.Equals(enc))
                {
                    ViewBag.Error = System.Configuration.ConfigurationManager.AppSettings["Validate007"];
                    return View();
                }
                var new_pwd_enc = FormsAuthentication.HashPasswordForStoringInConfigFile(new_pwd, "SHA1");
                user.Password = new_pwd_enc;
                UpdateModel(user);
                db.SaveChanges();
                FreebieEvent.UserUpdateEvent(Permission.staff_profile_page_id, "A04");
            }
            return RedirectToAction("StaffProfile");
        }

        public void init_dropdown(User u)
        {
            
            string role_cd = Session["Role"].ToString();
            string user_no = Session["User_No"].ToString();
            User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));

            IEnumerable<Dept> depts = db.Depts.Where(x => !x.Dept_Cd.Equals("MT")).OrderBy(s => s.Dept_Cd); 
            if (!role_cd.Equals("AM"))
            {
                depts = depts.Where(x => x.Dept_Cd.Equals(current_user.Dept_Cd));
            }

            var depts_selectable = new SelectList(depts, "Dept_Cd", "Dept_Name_En", u.Dept_Cd);
            ViewBag.Dept_Cd = depts_selectable;
            string selected_status_cd = "";
            if (!string.IsNullOrWhiteSpace(u.Status_Cd))
            {
                selected_status_cd = u.Status_Cd.Trim();
            }
            ViewBag.Status_Cd = new SelectList(db.Statuses.Where(x => x.Status_Type.Equals("User")).OrderBy(s => s.Status_Cd), "Status_Cd", "Status_Name_En", selected_status_cd);
            
            
            for (int i = 1; i <= 20; i++) {
                var item = new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString(),
                    Selected = (u.Group_Id == i)
                };
                if (role_cd.Equals("AM"))
                {
                    groups.Add(item);
                }
                else
                {
                    if (role_cd.Equals("SU") && i == current_user.Group_Id)
                    {
                        groups.Add(item);
                    }
                }
            }
                ViewBag.Group_Id = groups;

        }
    }
}
