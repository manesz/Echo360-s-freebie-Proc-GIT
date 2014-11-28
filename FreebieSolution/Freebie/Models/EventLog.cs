using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Event_Log")]
    public class EventLog
    {
        [Key]
        public int Event_Id { get; set; }
        public System.DateTime Event_Dttm { get; set; }
        public string User_No { get; set; }
        public string Account_No { get; set; }
        public byte? Page_Id { get; set; }
        public string Action_Cd { get; set; }
        public string Mobile_Number { get; set; }
        public string Identification_Number { get; set; }
        public string Account_Status_Cd { get; set; }
        public string Error_Msg { get; set; }

        [ForeignKey("Action_Cd")]
        public virtual EventAction EventAction { get; set; }

        [ForeignKey("Page_Id")]
        public virtual Page Page { get; set; }


        public EventLog()
        {
            this.Event_Dttm = DateTime.Now; 
        }
    }
}