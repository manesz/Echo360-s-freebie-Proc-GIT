using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
	public class Occupation
	{
		[Key]
		public string Occupation_Cd { get; set; }
		public string Occupation_Name_Th { get; set; }
		public string Occupation_Name_En { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

		public Occupation() {
			this.Created_Dttm = DateTime.Now;
			this.Updated_Dttm = DateTime.Now;
		}
	}
}