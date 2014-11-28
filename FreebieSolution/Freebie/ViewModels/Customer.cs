using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.ViewModels
{
    public class Customer
    {
        public int Account_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Phone_Number { get; set; }
        public string Identification_Number { get; set; }
        public string Email { get; set; }
        public string Status_Cd { get; set; }
    }
}