using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Personal_Income_Range")]
    public class PersonalIncomeRange
    {
        [Key]
		public string Personal_Income_Range_Cd { get; set; }
		public string Personal_Income_Range_Desc_Th { get; set; }
		public string Personal_Income_Range_Desc_En { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public PersonalIncomeRange() {
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}
    }
}