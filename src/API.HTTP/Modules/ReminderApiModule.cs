using System;
using System.Linq;
using Akka.Actor;
using ForgetMeNot.API.HTTP.ErrorHandling;
using ForgetMeNot.API.HTTP.BootStrap;
using ForgetMeNot.API.HTTP.Models;
using ForgetMeNot.Messages;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;

namespace ForgetMeNot.API.HTTP.Modules
{
	public class ReminderApiModule : NancyModule
	{
	    private readonly ActorSystem _actorSystem;
	    private static readonly ILog Logger = LogManager.GetLogger("ReminderService.API.HTTP.ReminderApiModule");

		//todo: look at making the actions Async operations
		public ReminderApiModule (ActorSystem actorSystem) 
			: base("/v1/reminders")
		{
		    _actorSystem = actorSystem;

			// Get the current state of a reminder by id
			Get ["/{reminderId}"] = SafeHandler(HandleGetReminderRequest);

			// Get the current state of reminders by tag
			Get["/active-by-tag/{tag}"] = SafeHandler(HandleGetRemindersByTagRequest);

			// Schedule a reminder
			Post["/"] = SafeHandler(HandlePostReminderRequest);

			// Cancel a reminder
			Delete ["/{reminderId}"] = SafeHandler(HandleDeleteReminderRequest);
		}

		private dynamic HandleGetReminderRequest(dynamic parameters)
		{
			Guid reminderId;
			string idstring = parameters.reminderId.ToString();
			if(!Guid.TryParse(idstring, out reminderId)){
				return Response.AsJson(new { errorMessage = "Not a valid Reminder Id: " + idstring }, HttpStatusCode.BadRequest);
			}

			var request = new QueryResponse.GetReminderState(reminderId);
		    var response = _bus.Send(request);
			if(response.HasValue)
				return Response.AsJson(response.Value);

			return Response.AsJson(
				new { errorMessage = string.Format("Did not find reminder with Id [{0}]", reminderId) },
				HttpStatusCode.NotFound);
		}

		private dynamic HandleGetRemindersByTagRequest(dynamic parameters)
		{
			var request = new QueryResponse.GetActiveRemindersByTag(parameters.tag);
			var response = _bus.Send(request);
			return Response.AsJson(response.HasValue ? response.Value : null);
		}
			
		private dynamic HandlePostReminderRequest(dynamic parameters)
		{
			string errormessage;
			var model = DeserializeBodyAsScheduleReminder(out errormessage);
			if (errormessage != null)
				return Response.AsJson(new { errorMessage = errormessage }, HttpStatusCode.BadRequest);					

			var result = this.Validate(model);

			if (!result.IsValid) {
				var errors = result.Errors.Values.SelectMany(ee => ee.Select(e => new { errorMessage = e.ErrorMessage}));
				return Response.AsJson(errors, HttpStatusCode.BadRequest);
			}

			var schedule = model.BuildScheduleMessage(Guid.NewGuid());

			_bus.Send(schedule);

			var scheduleRes = new ScheduledResponse{ReminderId = schedule.ReminderId};
			return Response.AsJson(scheduleRes, HttpStatusCode.Created);	
		}

		private dynamic HandleDeleteReminderRequest(dynamic parameters)
		{
			Guid reminderId;
			bool parsed = Guid.TryParse(parameters.reminderId, out reminderId);
			if(!parsed || reminderId == Guid.Empty) 
			{
				return Response.AsJson(
					new { errorMessage = string.Format("ReminderId [{0}] is not valid.", reminderId) }, 
					HttpStatusCode.BadRequest);
			}

			//do we need to make sure that the reminderId exists and fail if it doesn't?
			//or can we just ignore the fact that the reminder does not exist?
			_bus.Send(new ReminderMessage.Cancel(reminderId));

			return HttpStatusCode.NoContent;		
		}

		private Func<dynamic, dynamic> SafeHandler(Func<dynamic, dynamic> handler)
		{
			return parameters =>
			{                        
				try
				{
					return handler(parameters);                                           
				}
				catch (Exception ex)
				{ 
					return RequestProcessing.ErrorProcessing(this.Context, ex);			
				}
			};
		}

		private ScheduleReminder DeserializeBodyAsScheduleReminder(out string errorMessage)
		{
		    errorMessage = null;

			try 
			{
				return this.Bind<ScheduleReminder>();
			}
			catch (Exception ex) 
			{
				errorMessage = ex.Message;
				return null;			
			}
		}

		public void Handle (SystemMessage.InitializationCompleted msg)
		{
			_systemHasInitialized = true;
		}
	}
}

