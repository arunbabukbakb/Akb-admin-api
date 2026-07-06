using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Data.Repository
{
	public class MobileService
	{
		private readonly string _accountSid;
		private readonly string _authToken;
		private readonly string _twilioPhoneNumber;

		public MobileService(string accountSid, string authToken, string twilioPhoneNumber)
		{
			_accountSid = accountSid;
			_authToken = authToken;
			_twilioPhoneNumber = twilioPhoneNumber;
			TwilioClient.Init(_accountSid, _authToken);
		}

		public async Task SendSmsAsync(string toPhoneNumber, string message)
		{
			var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
			{
				From = new PhoneNumber(_twilioPhoneNumber),
				Body = message
			};
			await MessageResource.CreateAsync(messageOptions);
		}

		public async Task SendWhatsAppMessageAsync(string toPhoneNumber, string message)
		{
			var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{toPhoneNumber}"))
			{
				From = new PhoneNumber($"whatsapp:{_twilioPhoneNumber}"),
				Body = message
			};
			await MessageResource.CreateAsync(messageOptions);
		}
	}
}
