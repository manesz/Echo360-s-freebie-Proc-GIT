using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    public class Page
    {
        [Key]
        public byte Page_Id { get; set; }
        public string Page_Name_Th { get; set; }
        public string Page_Name_En { get; set; }
        public string Path { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        public Page()
        {
            this.Created_Dttm = DateTime.Now;
            this.Updated_Dttm = DateTime.Now;
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
        
        }
    }
}