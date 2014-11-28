using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.ViewModels
{
    public class TrialQuota
    {
        public int? no_trial_used { get; set; }
        public int? no_trial_acc { get; set; }

        public int? trial_limit_total { get; set; }
        public int? trial_dur_val { get; set; }

        public bool? trial_enable_flag { get; set; }
    }
}