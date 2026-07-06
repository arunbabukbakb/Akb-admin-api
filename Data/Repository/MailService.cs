using SendGrid;
using SendGrid.Helpers.Mail;

namespace Data.Repository
{
	public class MailService
	{
		private readonly string _apiKey;

		public MailService(string apiKey)
		{
			_apiKey = apiKey;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
		{
			var client = new SendGridClient(_apiKey);
			var from = new EmailAddress("your-email@example.com", "Your Name");
			var to = new EmailAddress(toEmail);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
			await client.SendEmailAsync(msg);
		}
	}
}
