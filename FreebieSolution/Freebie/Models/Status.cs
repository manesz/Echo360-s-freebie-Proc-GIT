using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
	public class Status
	{
        [Key, Column(Order = 0)]
		public string Status_Type { get; set; }
        [Key, Column(Order = 1)]
		public string Status_Cd { get; set; }

		public string Status_Name_Th { get; set; }
		public string Status_Name_En { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public Status() {
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}
	}
}