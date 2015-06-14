using System;
using System.Net;
using System.Text;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;
using RestSharp;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class HTTPDelivery : IDeliverReminders
	{
		private readonly ILog Logger = LogManager.GetLogger(typeof(HTTPDelivery));
		private readonly IRestClient _restClient;

		public HTTPDelivery (IRestClient restClient)
		{
			Ensure.NotNull (restClient, "restClient");
			_restClient = restClient;
		}

		public void Send(ReminderMessage.Schedule dueReminder, string url, Action<ReminderMessage.Schedule> onSuccessfulSend, Action<ReminderMessage.Schedule, string> onFailedSend)
		{
			var request = new RestRequest (url, Method.POST)
			{ RequestFormat = DataFormat.Json }
				// since our payload is already valid JSON, we do not want to use the AddBody(...) method as this will JSONify our Json string and we get malformed Json as a result.
				// just add a body parameter directly to avoid this serializations step.
				.AddParameter ("application/json", Encoding.UTF8.GetString (dueReminder.Payload), ParameterType.RequestBody);

			_restClient.PostAsync (request, (response, handle) => {
				if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
					onSuccessfulSend(dueReminder);
				else if (response.ResponseStatus != ResponseStatus.Completed)
					onFailedSend(dueReminder, response.ErrorMessage);
				else
					onFailedSend(dueReminder, response.ErrorMessage + " - " + response.StatusDescription);
			});
		}
	}
}

