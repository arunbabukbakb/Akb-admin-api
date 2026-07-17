using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface INotificationService
	{
		Task SendSmsAsync(string toPhoneNumber, string message);
		Task SendWhatsAppMessageAsync(string toPhoneNumber, string message);
		Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent);
		Task SendNotificationAsync(NotificationRequest request, string url);
		Task SendNotificationToRolesAsync(string roles, string title, string body, string? url = null, string? type = null);
		Task SendNotificationToUserAsync(int userId, string title, string body, string? url = null, string? type = null);
	}
}
