using Data.Repository.IRepository;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Options;
using Models.ViewModels;

namespace Data.Repository
{
	public class NotificationService : INotificationService
	{
		private readonly MobileService _mobileService;
		private readonly MailService _mailService;
		private readonly AppSettings _mySettings;

		public NotificationService(MobileService mobileService, MailService mailService, IOptions<AppSettings> mySettings)
		{
			//mailService = _mailService ?? throw new ArgumentNullException(nameof(mailService));
			//mobileService = _mobileService ?? throw new ArgumentNullException();
			_mySettings = mySettings.Value;
		}
		public async Task SendSmsAsync(string toPhoneNumber, string message)
		{
			await _mobileService.SendSmsAsync(toPhoneNumber, message);
		}

		public async Task SendWhatsAppMessageAsync(string toPhoneNumber, string message)
		{
			await _mobileService.SendWhatsAppMessageAsync(toPhoneNumber, message);
		}

		public async Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
		{
			await _mailService.SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
		}

		public async Task SendNotificationAsync(NotificationRequest request, string url)
		{
			var message = new Message()
			{
				Token = request.Token,
				Notification = new Notification
				{
					Title = request.Title,
					Body = request.Body
				},
				Data = new Dictionary<string, string>
				{
					// Add more custom data if needed
					{ "click_action", "FLUTTER_NOTIFICATION_CLICK" },
					{ "url", $"{_mySettings.AppUrl}{url}" },
					{"functionUrl",url },
					{"type",request.Type}
				}
			};

			string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
			// Handle response if needed
		}
	}
}
