using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Email_Request")]
    public class EmailRequest
    {
        [Key]
        public string Email { get; set; }
        //public string Secret { get; set; }
        public System.DateTime Expired_Dttm { get; set; }
    }
}