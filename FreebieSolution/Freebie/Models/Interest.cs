using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
	public class Interest
	{
		[Key]
		public string Interest_Cd { get; set; }
		public string Interest_Name_Th { get; set; }
		public string Interest_Name_En { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public Interest() {
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}
	}
}