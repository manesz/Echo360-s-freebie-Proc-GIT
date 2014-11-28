using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
    [Table("SMSRegistration_Log")]
    public class SmsRegistrationLog
    {
        [Key, Column(Order = 0)]
        public string Mobile_Number { get; set; }
        [Key, Column(Order = 1)]
        public System.DateTime Created_Dttm { get; set; }
        public string RQ_Keyword { get; set; }
        public string RQ_Content { get; set; }
        public string RQ_Msg { get; set; }
        public string Result { get; set; }

        public SmsRegistrationLog()
        {
            this.Created_Dttm = DateTime.Now;
            this.RQ_Content = "";
            this.RQ_Keyword = "";
            this.RQ_Msg = "";
            this.Result = "";
        }
    }
}