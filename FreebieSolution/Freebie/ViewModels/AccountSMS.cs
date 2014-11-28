using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.ViewModels
{
    public class AccountSMS
    {
        public int? Account_Id { get; set; }
        public System.DateTime? Activation_Dttm { get; set; }
        public string Mobile_Number { get; set; }
    }
}