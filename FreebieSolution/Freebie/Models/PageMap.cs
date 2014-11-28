using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Page_Map")]
    public class PageMap
    {
        [Key, Column(Order = 0)]
        public byte Page_Id { get; set; }
        [Key, Column(Order = 1)]
        public string Dept_Cd { get; set; }
        [Key, Column(Order = 2)]
        public string Role_Cd { get; set; }

        public string View_All_Flag { get; set; }
        public string Full_Access_Flag { get; set; }
        public string Allow_Update_Flag { get; set; }

        [ForeignKey("Page_Id")]
        public virtual Page Page { get; set; }
        [ForeignKey("Dept_Cd")]
        public virtual Dept Dept { get; set; }
        [ForeignKey("Role_Cd")]
        public virtual Role Role { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        public PageMap()
        {
            this.Created_Dttm = DateTime.Now;
            this.Updated_Dttm = DateTime.Now;
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
        }
    }
}