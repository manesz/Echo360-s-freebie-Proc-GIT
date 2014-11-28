using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Account_Quota")]
    public class AccountQuota
    {
        [Key, Column(Order = 0)]
        public int Account_Id { get; set; }
        [Key, Column(Order = 1)]
        public string Quota_Cd { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        [ForeignKey("Account_Id")]
        public virtual Account Account { get; set; }
        [ForeignKey("Quota_Cd")]
        public virtual Quota Quota { get; set; }

        public AccountQuota() {
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Created_Dttm = DateTime.Now;
            this.Updated_Dttm = DateTime.Now;
        }
    }
}