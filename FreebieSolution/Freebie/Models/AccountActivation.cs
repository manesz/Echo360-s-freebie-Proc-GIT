using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Account_Activation")]
    public class AccountActivation
    {
        [Key]
        public System.DateTime Date { get; set; }
        public int? No_Max_Activation { get; set; }
        public int? No_Activation { get; set; } //100
        public int? No_Activation_Acc { get; set; }
        public int? No_Activation_Pending { get; set; } //1
        public int? No_Manual_Activation { get; set; }

        //public int version_no { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }
       
    }
}