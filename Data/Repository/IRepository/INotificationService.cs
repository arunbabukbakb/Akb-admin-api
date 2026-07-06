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
	}
}
