using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    public class Role
    {
        [Key]
        public string Role_Cd { get; set; }
        public string Role_Name_Th { get; set; }
        public string Role_Name_En { get; set; }
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }
    }
}