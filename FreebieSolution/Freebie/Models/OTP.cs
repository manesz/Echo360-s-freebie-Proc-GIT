using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Otp")]
    public class OTP
    {
        [Key]                                  
        public string PhoneNumber { get; set; }
        public string Secret { get; set; }
        public int Counter { get; set; }
        public System.DateTime Expired_Dttm { get; set; }
    }
}