using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.ViewModels
{
    public class SalesPerf
    {
        public string user_no { get; set; }
        public System.Nullable<DateTime> reg_date { get; set; }
        public int? active_low { get; set; }
        public int? active_mid { get; set; }
        public int? active_high { get; set; }
        public int? pending_low { get; set; }
        public int? pending_mid { get; set; }
        public int? pending_high { get; set; }
        public int? user_total { get; set; }
    }
}