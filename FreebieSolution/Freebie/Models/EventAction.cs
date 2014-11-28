using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Event_Action")]
    public class EventAction
    {
        [Key]
        public string Action_Cd { get; set; }
        public string Action_Name_Th { get; set; }
        public string Action_Name_En { get; set; }

        public string Create_By { get; set; }
        public string Update_By { get; set; }
        public System.DateTime? Create_Dttm { get; set; }
        public System.DateTime? Update_Dttm { get; set; }

        public EventAction()
        {
            this.Create_Dttm = DateTime.Now;
            this.Update_Dttm = DateTime.Now;
            this.Create_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Update_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
        }
    }
}