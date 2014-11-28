using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace Freebie.Models
{
	public class Account
	{    
		[Key]
		public int Account_Id { get; set; }
		public string Account_No { get; set; }
        //[Required(ErrorMessage = "กรุณาระบุชื่อผู้ใช้")]
		public string User_Name { get; set; }
       // [Required(ErrorMessage = "กรุณาระบุรหัสผ่าน")]
		public string Password { get; set; }
        //[Required(ErrorMessage = "กรุณาระบุชื่อจริง")]
		public string First_Name { get; set; }
        //[Required(ErrorMessage = "กรุณาระบุนามสกุล")]
		public string Last_Name { get; set; }
		public byte? Day_Of_Birth { get; set; }
		public byte? Month_Of_Birth { get; set; }
		public int? Year_Of_Birth { get; set; }
        [Required(ErrorMessage = "กรุณาระบุเพศ")]
		public string Gender_Cd { get; set; }		 
		public string Status_Cd { get; set; }
		public string Income_Range_Cd { get; set; }
        public string Personal_Income_Range_Cd { get; set; }		
		public string Occupation_Cd { get; set; }		
		public string Education_Cd { get; set; }		
		public string Marital_Status_Cd { get; set; }
        public string Children_Flag { get; set; }
        public int? Year_Of_Birth_Child1 { get; set; }
        public int? Year_Of_Birth_Child2 { get; set; }
        public int? Year_Of_Birth_Child3 { get; set; }
		public string Dummy_Flag { get; set; }
		public char Email_Verify_Flag { get; set; }
		public string Identification_Number { get; set; }
		public char Identification_Verify_Flag { get; set; }
		public string First_Quota_Cd { get; set; }
		public byte? First_Quota_Freq_Val { get; set; }
		public byte? First_Quota_Dur_Val { get; set; }
		public string Channel_Cd { get; set; }	

		public System.DateTime? Registration_Dttm { get; set; }
		public System.DateTime? Activation_Dttm { get; set; }
		public System.DateTime? First_Call_Dttm { get; set; }
        public System.DateTime? Interact_Profile_Dttm { get; set; }

        public string First_Mobile_Number { get; set; }
        [Column("Zipcode")]
        public string ZipCode { get; set; }
        [Column("Areacode")]
        public string AreaCode { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

        public string Staff_No { get; set; }

		[ForeignKey("Marital_Status_Cd")]
		public virtual MaritalStatus MaritalStatus { get; set; }
        [ForeignKey("Education_Cd")]
		public virtual Education Education { get; set; }
        [ForeignKey("Occupation_Cd")]
		public virtual Occupation Occupation { get; set; }
        [ForeignKey("Income_Range_Cd")]
		public virtual IncomeRange IncomeRange { get; set; }
        [ForeignKey("Personal_Income_Range_Cd")]
        public virtual PersonalIncomeRange PersonalIncomeRange { get; set; }

        [ForeignKey("Gender_Cd")]
		public virtual Gender Gender { get; set; }
        [ForeignKey("Channel_Cd")]
        public virtual Channel Channel { get; set; }

        //[ForeignKey("Status_Cd")]
        //public virtual Status Status { get; set; }


		public Account() {
			
			//this.Created_Dttm = DateTime.Now;
			//this.Updated_Dttm = DateTime.Now;
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
			//this.Registration_Dttm = DateTime.Now;
			this.Activation_Dttm = null;
			this.First_Call_Dttm = null;
		}

        public string Status() {
            EchoContext db = new EchoContext();
            var status = db.Statuses.Where(x => x.Status_Type.Equals("Account")).Where(x => x.Status_Cd.Equals(this.Status_Cd)).SingleOrDefault();
            if (status == null) { return ""; }
            else { return status.Status_Name_Th; }
        }

       
	}
}