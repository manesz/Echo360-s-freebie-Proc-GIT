using System.Web.Mvc;

namespace Freebie.Areas.Backend
{
	public class BackendAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Backend";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Backend_default",
				"Backend/{controller}/{action}/{id}",
				new { action = "StaffProfile", controller = "User" ,id = UrlParameter.Optional }
			);
		}
	}
}
