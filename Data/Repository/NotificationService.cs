using Data.Repository.IRepository;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repository
{
	public class NotificationService : INotificationService
	{
		private readonly MobileService _mobileService;
		private readonly MailService _mailService;
		private readonly AppSettings _mySettings;
		private readonly IUnitOfWork _db;
		private readonly ILogger<NotificationService> _logger;

		public NotificationService(
			MobileService mobileService, 
			MailService mailService, 
			IOptions<AppSettings> mySettings,
			IUnitOfWork db,
			ILogger<NotificationService> logger)
		{
			_mobileService = mobileService;
			_mailService = mailService;
			_mySettings = mySettings.Value;
			_db = db;
			_logger = logger;
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
			try
			{
				if (string.IsNullOrEmpty(request.Token))
				{
					_logger.LogWarning("Firebase message token is empty.");
					return;
				}

				if (FirebaseApp.DefaultInstance == null)
				{
					_logger.LogWarning("FirebaseApp is not initialized (notification.json is missing or invalid). Skipping notification delivery.");
					return;
				}

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
						{ "click_action", "FLUTTER_NOTIFICATION_CLICK" },
						{ "url", $"{_mySettings.AppUrl}{url}" },
						{ "functionUrl", url ?? "" },
						{ "type", request.Type ?? "" }
					}
				};

				string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
				_logger.LogInformation("Firebase notification sent successfully: {Response}", response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send Firebase notification to token {Token}", request.Token);
			}
		}

		public async Task SendNotificationToRolesAsync(string roles, string title, string body, string? url = null, string? type = null)
		{
			if (string.IsNullOrEmpty(roles)) return;

			var roleCodes = roles.Split(',')
				.Select(r => r.Trim())
				.Where(r => !string.IsNullOrEmpty(r))
				.ToList();

			if (!roleCodes.Any()) return;

			var users = _db.User.GetAll(
				u => u.Status && !u.IsDeleted && !string.IsNullOrEmpty(u.FcmToken) && roleCodes.Contains(u.Role.CodeName),
				includeProperties: "Role"
			);

			foreach (var user in users)
			{
				var request = new NotificationRequest
				{
					Token = user.FcmToken,
					Title = title,
					Body = body,
					Type = type ?? ""
				};
				await SendNotificationAsync(request, url ?? "");
			}
		}

		public async Task SendNotificationToUserAsync(int userId, string title, string body, string? url = null, string? type = null)
		{
			var user = _db.User.GetFirstOrDefault(u => u.Id == userId);
			if (user != null && !string.IsNullOrEmpty(user.FcmToken))
			{
				var request = new NotificationRequest
				{
					Token = user.FcmToken,
					Title = title,
					Body = body,
					Type = type ?? ""
				};
				await SendNotificationAsync(request, url ?? "");
			}
		}
	}
}
