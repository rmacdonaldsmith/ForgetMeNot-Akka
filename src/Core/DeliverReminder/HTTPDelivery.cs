using System;
using System.Text;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using log4net;
using RestSharp;

namespace ForgetMeNot.Core.DeliverReminder
{
	public class HttpDelivery : TypedActor, IHandle<ReminderMessage.Due>
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpDelivery));
		private readonly IRestClient _restClient;

	    public HttpDelivery (IRestClient restClient)
		{
			Ensure.NotNull (restClient, "restClient");

			_restClient = restClient;
		}

	    public void Handle(ReminderMessage.Due due)
	    {
	        var request = new RestRequest(due.Reminder.DeliveryUrl, Method.POST) {RequestFormat = DataFormat.Json}
	            // since our payload is already valid JSON, we do not want to use the AddBody(...) method as this will JSONify our Json string and we get malformed Json as a result.
	            // just add a body parameter directly to avoid this serializations step.
	            .AddParameter("application/json", Encoding.UTF8.GetString(due.Reminder.Payload), ParameterType.RequestBody);

            _restClient.PostAsync(request, (response, handle) =>
                {
                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                        Sender.Tell(new DeliveryMessage.Delivered(due.ReminderId, SystemTime.UtcNow()));
                    else if (response.ResponseStatus != ResponseStatus.Completed)
                        Sender.Tell(new DeliveryMessage.NotDelivered(due.Reminder, response.ErrorMessage));
                    else
                        Sender.Tell(new DeliveryMessage.NotDelivered(due.Reminder,
                                                                               response.ErrorMessage + " - " +
                                                                               response.StatusDescription));
                });
	    }

        public static Func<IRestClient, Props> PropsFactory
	    {
	        get { return restClient => Props.Create(() => new HttpDelivery(restClient)); }
	    }
	}
}

