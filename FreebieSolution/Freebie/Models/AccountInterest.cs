using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
    [Table("Account_Interest")]
	public class AccountInterest
	{
        [Key]
		public int Account_Id { get; set; }
        [ForeignKey("Account_Id")]
        public virtual Account Account { get; set; }
        public bool I01_Food_Dining { get; set; }
        public bool I02_Night_Life { get; set; }
        public bool I03_Entertainment { get; set; }
        public bool I04_Music_Movie { get; set; }
        public bool I05_Sports_Fitness { get; set; }
        public bool I06_Shopping_Fashion { get; set; }
        public bool I07_Health_Beauty { get; set; }
        public bool I08_Travel { get; set; }
        public bool I09_Pets { get; set; }
        public bool I10_Kids_Children { get; set; }
        public bool I11_Home_Living { get; set; }
        public bool I12_Finance_Investment { get; set; }
        public bool I13_Technology_Gadget { get; set; }
        public bool I14_Auto { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public AccountInterest() {
            // set false
            this.I01_Food_Dining = false;
            this.I02_Night_Life = false;
            this.I03_Entertainment = false;
            this.I04_Music_Movie = false;
            this.I05_Sports_Fitness = false;
            this.I06_Shopping_Fashion = false;
            this.I07_Health_Beauty = false;
            this.I08_Travel = false;
            this.I09_Pets = false;
            this.I10_Kids_Children = false;
            this.I11_Home_Living = false;
            this.I12_Finance_Investment = false;
            this.I13_Technology_Gadget = false;
            this.I14_Auto = false;

            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}

	}
}