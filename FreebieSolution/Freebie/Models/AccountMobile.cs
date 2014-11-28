using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
    [Table("Account_Mobile")]
	public class AccountMobile
	{
        public int Account_Id { get; set; }
        [ForeignKey("Account_Id")]
		public virtual Account Account { get; set; }
		[Key]
		public string Mobile_Number { get; set; }
		public bool Primary_Flag { get; set; }
		public string  Status_Cd { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public AccountMobile() {
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}

        public string Status()
        {
            EchoContext db = new EchoContext();
            var status = db.Statuses.Where(x => x.Status_Type.Equals("Mobile")).Where(x => x.Status_Cd.Equals(this.Status_Cd)).SingleOrDefault();
            if (status == null) { return ""; }
            else { return status.Status_Name_Th; }
        }
	}
}