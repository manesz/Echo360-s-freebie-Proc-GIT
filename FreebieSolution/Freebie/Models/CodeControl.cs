using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
	public class CodeControl
	{
		[Key]
		public string Prefix_Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Sequence_No { get; set; }
		public int Suffix_Length { get; set; }

	}
}