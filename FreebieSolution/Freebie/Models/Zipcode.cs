using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freebie.Models
{
    public class Zipcode
    {
        [Key, Column(Order = 0)]
        public string ZipCode { get; set; }
        [Key, Column(Order = 1)]
        public string AreaCode { get; set; }
        public string District { get; set; }
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }
    }
}