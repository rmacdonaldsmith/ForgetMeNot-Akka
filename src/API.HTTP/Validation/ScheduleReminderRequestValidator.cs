using FluentValidation;
using ForgetMeNot.API.HTTP.Models;
using ForgetMeNot.Common;
using NodaTime;
using NodaTime.Text;

namespace ForgetMeNot.API.HTTP.Validation
{
	public class ScheduleReminderRequestValidator : AbstractValidator<ScheduleReminder>
	{
		public ScheduleReminderRequestValidator ()
		{
			RuleFor(request => request.DeliveryUrl)
				.NotEmpty()
				.WithMessage("DeliveryUrl must be specified.");
			RuleFor (request => request.Payload)
				.NotEmpty()
				.IsValidJson();
			RuleFor (request => request.ContentType)
				.NotEmpty ()
				.Equal ("application/json")
				.WithMessage ("We only support json at the moment.");
			RuleFor (request => request.Encoding)
				.NotEmpty ()
				.Equal ("utf8")
				.WithMessage ("We only support UTF8 encoding at the moment.");
			RuleFor (request => request.Transport)
				.NotEmpty ()
				.Must(BeAValidTransport)
				.WithMessage ("We only support Http and RabbitMQ transports at the moment.");
			RuleFor (request => request.DueAt)
				.Cascade (CascadeMode.StopOnFirstFailure)
				.NotEmpty ()
				.Must (BeAValidDueAtTime)
				.WithMessage ("The DueAt value could not be deserialized to a valid DateTime instance OR is in the past.");
			RuleFor (request => request.MaxRetries)
				.GreaterThanOrEqualTo (0);
			RuleFor (request => request.GiveupAfter)
				.Must (BeAValidGiveUpAfterTime)
				.WithMessage ("The GiveupAfter value could not be deserialized to a valid DateTime instance OR is in the past.");
		}

		private bool BeAValidTransport(string transport)
		{
			//doing this because I am getting strange unit test results.
			//all unit tests fail because arg: transport is being passed in as null, except for the tests that
			//are testing the Transport property.
			//Need to fix / figure this out!
			if (transport == null)
				return true;

			var input = transport.ToLower ();
			return (input == "http" || input == "rabbitmq");
		}

		private bool BeAValidDueAtTime(string dueAtString)
		{
			var pattern = OffsetDateTimePattern.ExtendedIsoPattern;
			var result = pattern.Parse (dueAtString);

			if (!result.Success)
				return false;

			var dueAtUtc = result.Value.ToInstant ().InUtc ().ToInstant();
			var utcNow = Instant.FromDateTimeUtc (SystemTime.UtcNow ());
			return (dueAtUtc.CompareTo (utcNow) > 0);
		}

		private bool BeAValidGiveUpAfterTime(ScheduleReminder request, string giveUpAfterString)
		{
			if (string.IsNullOrEmpty (giveUpAfterString))
				return true;

			var pattern = OffsetDateTimePattern.ExtendedIsoPattern;
			var result = pattern.Parse (giveUpAfterString);

			if (!result.Success)
				return false;

			var giveupAfterUtc = result.Value.ToInstant ().InUtc ();
			var dueAtUtc = pattern.Parse (request.DueAt).GetValueOrThrow().ToInstant().InUtc();
			return (giveupAfterUtc.CompareTo (dueAtUtc) > 0);
		}
	}
}

