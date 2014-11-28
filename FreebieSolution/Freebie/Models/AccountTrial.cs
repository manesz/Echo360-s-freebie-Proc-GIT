using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Account_Trial")]
    public class AccountTrial
    {
        [Key]
        public System.DateTime Date { get; set; }
        public int? No_Max_Trial { get; set; }
        public int? No_Trial_Used { get; set; }
        public int? No_Trial_Used_Acc { get; set; }
        public bool? Trial_Avail_Flag { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }
    }
}