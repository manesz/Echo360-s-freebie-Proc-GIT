using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Caching;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Freebie.Libs;
using System.Diagnostics;

namespace Freebie
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class WebApiApplication : System.Web.HttpApplication
	{
        public static IScheduler scheduler = null;
		protected void Application_Start()
		{
            ISchedulerFactory scheduleFactory = new StdSchedulerFactory();
            scheduler = scheduleFactory.GetScheduler();
            scheduler.Start();
            
            // Add SMS Activation scheduler
            ActivationJob job = new ActivationJob();
            JobDetailImpl detail = new JobDetailImpl("ActivationJob", "SMS", job.GetType());
            string cronExp = System.Configuration.ConfigurationManager.AppSettings["ACTIVATION_SMS_CRON_EXP"];
            CronTriggerImpl trigger = new CronTriggerImpl("trigger1", "SMS", cronExp);
            
            scheduler.ScheduleJob(detail, trigger);
            Debug.WriteLine("...Scheduled: Activation SMS Job.......");

            AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
           
		}


        protected void Application_End(object sender, EventArgs e)
        {
            if (scheduler != null)
            {
                scheduler.Shutdown();
            }
        }
        public void Session_End(Object sender, EventArgs E)
        {
            
        }
	}
}