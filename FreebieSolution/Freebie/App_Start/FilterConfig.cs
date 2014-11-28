using System.Web;
using System.Web.Mvc;
using Freebie.Libs;

namespace Freebie
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
            filters.Add(new ElmahHandledErrorLoggerFilter());
			filters.Add(new HandleErrorAttribute());
		}
	}
}