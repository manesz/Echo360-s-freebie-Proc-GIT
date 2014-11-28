using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Diagnostics;
using System.Threading;

namespace Freebie.Libs
{
    public class ActivationJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("........Starting Schedule job at " + DateTime.Now);
            ActivationSMS.send_sms();
            Debug.WriteLine("........Finished Schedule job at " + DateTime.Now);
        }
    }
}