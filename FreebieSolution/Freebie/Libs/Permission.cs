using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Freebie.Models;

namespace Freebie.Libs
{
    public static class Permission
    {
        //frontend
        public static byte f_user_home_page_id = 1;
        public static byte f_cust_regis_page_id = 2;
        public static byte f_cust_info_page_id = 3;
        public static byte f_update_number_page_id = 4;
        public static byte f_update_profile_page_id = 5;
        public static byte f_update_username_page_id = 6;
        public static byte f_update_password_page_id = 7;


        //backend
        public static byte staff_home_page_id = 10;
        public static byte sales_perf_page_id = 11;
        public static byte register_page_id = 12;
        public static byte search_cust_page_id = 13;
        public static byte staff_profile_page_id = 14;
        public static byte sup_acct_page_id = 15;
        public static byte staff_acct_page_id = 16;
        public static byte base_quota_page_id = 17;
        public static byte free_trial_page_id = 18;
        public static byte activation_page_id = 19;
        public static byte cust_profile_page_id = 20;
        public static byte cust_numbers_page_id = 21;
        public static byte update_cust_profile_page_id = 22;
        public static byte update_cust_username_page_id = 23;

        public static bool has_permission(byte page_id)
        {
            try
            {
                Hashtable permissions = new Hashtable();
                permissions = (Hashtable)HttpContext.Current.Session["Permissions"];
                string page_id_str = page_id.ToString();
                Hashtable page_permissions = (System.Collections.Hashtable)permissions[page_id_str];
                if (page_permissions != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool access_menu(byte page_id)
        {
            try
            {
                bool result = has_permission(page_id);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool allow_update(byte page_id)
        {
            if (has_permission(page_id))
            {
                bool result = false;
                Hashtable permissions = new Hashtable();
                permissions = (Hashtable)HttpContext.Current.Session["Permissions"];
                string page_id_str = page_id.ToString();
                Hashtable page_permissions = (System.Collections.Hashtable)permissions[page_id_str];
                result = (bool)page_permissions["Allow_Update"];
                return result;
            }
            else
            {
                return false;
            }
        }
        public static bool view_all(byte page_id)
        {
            if (has_permission(page_id))
            {
                bool result = false;
                Hashtable permissions = new Hashtable();
                permissions = (Hashtable)HttpContext.Current.Session["Permissions"];
                string page_id_str = page_id.ToString();
                Hashtable page_permissions = (System.Collections.Hashtable)permissions[page_id_str];
                result = (bool)page_permissions["View_All"];
                return result;
            }
            else
            {
                return false;
            }
        }

        public static bool access_all(byte page_id)
        {
            if (has_permission(page_id))
            {
                bool result = false;
                Hashtable permissions = new Hashtable();
                permissions = (Hashtable)HttpContext.Current.Session["Permissions"];
                string page_id_str = page_id.ToString();
                Hashtable page_permissions = (System.Collections.Hashtable)permissions[page_id_str];
                result = (bool)page_permissions["Access_All"];
                return result;
            }
            else
            {
                return false;
            }
        }

        
        

        public static bool access_admin_config_menu()
        {
            bool result = false;
            result = (access_menu(base_quota_page_id) || access_menu(free_trial_page_id) || access_menu(activation_page_id));

            return result;
        }

        public static bool access_users_menu()
        {
            bool result = false;
            result = (access_menu(staff_acct_page_id) || access_menu(sup_acct_page_id) || access_menu(staff_profile_page_id));
            return result;
        }

        public static bool access_register_menu()
        {
            bool result = false;
            result = access_menu(register_page_id);
            return result;
        }

        public static bool access_report_menu()
        {
            bool result = false;
            result = access_menu(sales_perf_page_id);
            return result;
        }

        public static bool access_search_customer_menu()
        {
            bool result = false;
            result = (access_menu(search_cust_page_id));
            return result;
        }

        public static bool access_register_customer_menu()
        {
            bool result = false;
            result = access_menu(register_page_id);
            return result;
        }

        public static bool can_update_this_staff(User u)
        {
            // can CRUD this stff

            bool result = true;
            using (var db = new EchoContext())
            {
                string user_no = HttpContext.Current.Session["User_No"].ToString();
                User current_user = db.Users.SingleOrDefault(x => x.User_No.Equals(user_no));

                if (!current_user.Role_Cd.Equals("AM"))
                {
                    if (current_user.Role_Cd.Equals("SU"))
                    {
                        if (u.Role_Cd.Equals("ST"))
                        {
                            if ((u.Group_Id != current_user.Group_Id) || (!u.Dept_Cd.Equals(current_user.Dept_Cd)))
                            {
                                result = false; // only staff in user's group/dept
                            }
                        }
                        else
                        {
                            if (!u.User_No.Equals(current_user.User_No))
                            {
                                result =  false; // self edit only
                            }
                        }

                    }

                    if (current_user.Role_Cd.Equals("ST"))
                    {
                        if (!u.User_No.Equals(current_user.User_No))
                        {
                            result = false; // self edit only
                        }
                    }
                } 
            }

            return result;
        }
    }
}