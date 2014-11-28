using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
    public class User
    {
        [Key]
        public int User_Id { get; set; }
        public string User_No { get; set; }
        public string User_Name { get; set; }
        public string Password { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Status_Cd { get; set; }
        public string Dept_Cd { get; set; }
        public string Role_Cd { get; set; }
        public byte? Group_Id { get; set; }
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        [ForeignKey("Dept_Cd")]
        public virtual Dept Dept { get; set; }
        [ForeignKey("Role_Cd")]
        public virtual Role Role { get; set; }

        public User()
        {
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Created_Dttm = DateTime.Now;
            this.Updated_Dttm = DateTime.Now;
        }
        public string Status()
        {
            EchoContext db = new EchoContext();
            var status = db.Statuses.Where(x => x.Status_Type.Equals("User")).Where(x => x.Status_Cd.Equals(this.Status_Cd)).SingleOrDefault();
            if (status == null) { return ""; }
            else { return status.Status_Name_En; }
        }

        
    }
}