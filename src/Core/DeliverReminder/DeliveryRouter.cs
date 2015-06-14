using System;
using System.Collections.Generic;
using Akka.Actor;
using ForgetMeNot.Common;
using ForgetMeNot.Messages;
using ForgetMeNot.Router;
using log4net;

namespace ForgetMeNot.Core.DeliverReminder
{
	public enum DeliveryTransport
	{
		None,
		HTTP,
		RabbitMq
	}

	public class DeliveryRouter : TypedActor
	{
		private readonly ILog Logger = LogManager.GetLogger(typeof(DeliveryRouter));
		private readonly string _deadLetterUrl;
		private readonly ISendMessages _bus;

		private readonly IDictionary<DeliveryTransport, IDeliverReminders> _handlers = new Dictionary<DeliveryTransport, IDeliverReminders> ();
		private readonly IDictionary<ReminderMessage.TransportEnum, DeliveryTransport> _transportMap 
			= new Dictionary<ReminderMessage.TransportEnum, DeliveryTransport>
		{
			{ ReminderMessage.TransportEnum.http, DeliveryTransport.HTTP },
			{ ReminderMessage.TransportEnum.rabbitmq, DeliveryTransport.RabbitMq },
		};

		public DeliveryRouter (ISendMessages bus, string deadLetterUrl)
		{
			Ensure.NotNull (bus, "bus");
			Ensure.NotNullOrEmpty (deadLetterUrl, "deadLetterUrl");

			_bus = bus;
			_deadLetterUrl = deadLetterUrl;
		}

		public void AddHandler(DeliveryTransport transport, IDeliverReminders handler)
		{
			if(!_handlers.ContainsKey(transport))
				_handlers.Add(transport, handler);
		}

		public void Handle(ReminderMessage.Due due)
		{
			if (!_transportMap.ContainsKey(due.Reminder.Transport) || !_handlers.ContainsKey(_transportMap[due.Reminder.Transport])) 
			{
				var exception = new NotSupportedException(string.Format("Delivery transport not supported for reminder [{0}]", due.ReminderId));
				Logger.ErrorFormat(exception,
					new { logname = "delivery", disposition = "undeliverable", reminderid = due.ReminderId },
					"There are no reminder delivery handlers registered to deliver '{0}'", due.Reminder.DeliveryUrl);
     				throw exception;
			}

			var handler = _handlers[_transportMap[due.Reminder.Transport]];
			handler.Send(due.Reminder, due.Reminder.DeliveryUrl, OnSuccessfulDelivery, OnFailedDelivery);
		}

		private void OnSuccessfulDelivery(ReminderMessage.Schedule sentReminder)
		{
			Logger.InfoFormat (new { logname = "delivery", disposition = "delivered", reminderid =  sentReminder.ReminderId },
				"Reminder [{0}] was successfully delivered.", sentReminder.ReminderId);
			_bus.Send (new ReminderMessage.Delivered(sentReminder.ReminderId, SystemTime.UtcNow()));
		}

		private void OnFailedDelivery(ReminderMessage.Schedule failedReminder, string errorMessage)
		{
			Logger.ErrorFormatNoException(new { logname = "delivery", disposition = "undeliverable", reminderid =  failedReminder.ReminderId },
				"Reminder [{0}] could not be delivered: {1}", failedReminder.ReminderId, errorMessage);
			_bus.Send (new ReminderMessage.Undelivered(failedReminder, errorMessage));
		}
	}
}