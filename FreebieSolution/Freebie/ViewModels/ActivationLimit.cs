using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freebie.ViewModels
{
    public class ActivationLimit
    {
        public int? no_activation { get; set; }
        public int? no_activation_pending { get; set; }
        public int? no_activation_acc { get; set; }
        public int? no_activation_limit_total { get; set; }
        public int? no_activation_limit_daily { get; set; }
    }
}