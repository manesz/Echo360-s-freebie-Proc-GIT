using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Freebie.Models
{
    [Table("Account_Quota_Used_Cur")]
    public class AccountQuotaUsedCur
    {
        [Key, Column(Order = 0), DataType(DataType.Date)]
        public System.DateTime Date { get; set; }
        [Key, Column(Order = 1)]
        public int Account_Id { get; set; }
        [ForeignKey("Account_Id")]
        public virtual Account Account { get; set; }
        public byte Quota_Freq_Val { get; set; }
        public byte Quota_Dur_Val { get; set; }
        public byte Quota_Freq_Used_Val { get; set; }
        public bool Quota_Avail_Flag { get; set; }

        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public System.DateTime Created_Dttm { get; set; }
        public System.DateTime Updated_Dttm { get; set; }

        public AccountQuotaUsedCur()
        {
            this.Created_Dttm = DateTime.Now;
            this.Updated_Dttm = DateTime.Now;
            this.Created_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
            this.Updated_By = System.Configuration.ConfigurationManager.AppSettings["SystemUsername"];
        }
    }
}