using Mvc.Mailer;
using System.Web;

namespace Freebie.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer 	
	{
		public UserMailer()
		{
			MasterName="_Layout";
		}
		
		public virtual MvcMailMessage EnterNewPassword(string email, string username ,string http_enc)
		{
            ViewBag.Link = HttpContext.Current.Request.Url.ToString().Replace("ForgotPassword", "EnterNewPassword?Ref=") + http_enc;
            ViewBag.Username = username;

			return Populate(x =>
			{
                x.Subject = "รหัสผ่านใหม่เว็บไซต์ www.freebie.co.th";
                x.ViewName = "EnterNewPassword";
				x.To.Add(email);
			});
		}
 	}
}