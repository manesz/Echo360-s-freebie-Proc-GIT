using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Otp_Request")]
    public class OTPRequest
    {

        [Key, Column(Order = 0), DataType(DataType.Date)]
        public System.DateTime Date { get; set; }
        [Key, Column(Order = 1)]
        public string PhoneNumber { get; set; }
        public System.DateTime Last_Request_At { get; set; }
        public int Count { get; set; }
    }
}