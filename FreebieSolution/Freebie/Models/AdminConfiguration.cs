using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Admin_Configuration")]
    public class AdminConfiguration
    {
        public int Trial_Dur_Val { get; set; }
        public bool Trial_Enable_Flag { get; set; }
        
        public int Trial_Limit_Total { get; set; }
        public int No_Activation_Limit_Total { get; set; }
        public int No_Activation_Limit_Daily { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        [Key]
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        public Nullable<TimeSpan> Regist_Disable_StartTime { get; set; }
        public Nullable<TimeSpan> Regist_Disable_EndTime { get; set; }
    }
}