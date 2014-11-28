using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("ActivationSMS_Log")]
    public class ActivationSmsLog
    {
        [Key]
        public int Id { get; set; }
        public string Mobile_Number { get; set; }
        public System.DateTime Request_At { get; set; }
        public System.Nullable<System.DateTime> Response_At { get; set; }
        public string Response_Text { get; set; }
    }
}