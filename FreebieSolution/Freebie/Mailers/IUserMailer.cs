using Mvc.Mailer;

namespace Freebie.Mailers
{ 
    public interface IUserMailer
    {
			MvcMailMessage EnterNewPassword(string email, string username, string http_enc);
	}
}