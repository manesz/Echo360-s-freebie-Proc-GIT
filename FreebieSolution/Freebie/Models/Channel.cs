using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
	public class Channel
	{
		[Key]
		public string Channel_Cd { get; set; }
		public string Channel_Name_Th { get; set; }
		public string Channel_Name_En { get; set; }
		public string Created_By { get; set; }
		public string Updated_By { get; set; }
		public System.DateTime Created_Dttm { get; set; }
		public System.DateTime Updated_Dttm { get; set; }

        public static string web_channel()
        {
            return "WEB";
        }

        public static string ivr_channel()
        {
            return "IVR";
        }
	}
}