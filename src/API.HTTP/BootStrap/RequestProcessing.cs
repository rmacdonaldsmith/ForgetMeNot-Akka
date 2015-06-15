using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ForgetMeNot.API.HTTP.ErrorHandling;
using ForgetMeNot.Common;
using Nancy;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;
using log4net;

namespace ForgetMeNot.API.HTTP.BootStrap
{
	public class RequestProcessing
	{
		private static readonly ILog Logger = LogManager.GetLogger("ReminderService.API.HTTP.RequestProcessing");

		const string RequestTimerKey = "RequestTimer";
		const string RequestStartTimeKey = "RequestStartTime";

		public static Response PreProcessing(NancyContext context)
		{
			if (Thread.CurrentThread.Name == "Threadpool worker")
				Thread.CurrentThread.Name = "Threadpool worker " + Thread.CurrentThread.ManagedThreadId.ToString();
				
			context.Items[RequestTimerKey] = Stopwatch.StartNew();
			context.Items[RequestStartTimeKey] = SystemTime.UtcNow();
			return null;
		}

		public static void PostProcessing(NancyContext context)
		{
			Maybe<DateTime> maybeStarted = MaybeGetItem<DateTime>(RequestStartTimeKey, context);
			Maybe<Stopwatch> maybeTimer = MaybeGetItem<Stopwatch>(RequestTimerKey, context);

			LogRequest(maybeTimer, context);
			TallyHit(maybeStarted, maybeTimer, context);
		}

		public static Response ErrorProcessing(NancyContext context, Exception ex)
		{
			Maybe<DateTime> maybeStarted = MaybeGetItem<DateTime>(RequestStartTimeKey, context);
			Maybe<Stopwatch> maybeTimer = MaybeGetItem<Stopwatch>(RequestTimerKey, context);

			try
			{
				LogRequest(maybeTimer, context, ex);
				TallyHit(maybeStarted, maybeTimer, context);
			}
			catch (Exception ex2)
			{
				Logger.ErrorFormat (ex2, new { logname = "request" },
					"Error {0} occured while handling request error: {1}", ex2.GetType(), ex.GetType());			
			}

			return ErrorResponse.FromException(ex);
		}
			
		public static void LogRequest(Maybe<Stopwatch> stopWatch, NancyContext context, Exception ex = null)
		{
			int? statusCode = (context.Response != null) ? (int?)context.Response.StatusCode : null;

			string logmessage = "Request processing " + ((ex == null) ? "finished" : "failed");

			var logprops = new Dictionary<string, object> { { "logmessage", logmessage }, { "logname", "request" } };

			if (stopWatch.HasValue)
			{
				stopWatch.Value.Stop();
				logprops.Add("duration", stopWatch.Value.ElapsedTicks / 10);
			}

			if (statusCode != null)
				logprops.Add("status", (int)statusCode);

			if ((context.Request != null) && (!string.IsNullOrWhiteSpace(context.Request.Method)))
				logprops.Add("method", context.Request.Method);

			if ((context.Request != null) && (!string.IsNullOrWhiteSpace(context.Request.Path)))
				logprops.Add("url", context.Request.Path);

			if ((context.Request != null) && (context.Request.Headers != null))
			{
				logprops.Add("user-agent", context.Request.Headers.UserAgent ?? "(none)");
				logprops.Add("bodysize", context.Request.Headers.ContentLength);
			}

			SetOtHeaderProperties(logprops, context.Request.Headers);

			if (ex != null)
				Logger.Error(logprops, ex);
			else if (IsServiceFault(statusCode ?? 500))
				Logger.Error(logprops);
			else
				Logger.Info(logprops);
		}

		private static void TallyHit(Maybe<DateTime> maybeStarted, Maybe<Stopwatch> maybeTimer, NancyContext context, Exception ex = null)
		{
			int? statusCode = (context.Response != null) ? (int?)context.Response.StatusCode : null;

			string path = "??";
			if (statusCode.HasValue && (statusCode.Value != 404) && (context.ResolvedRoute != null) && (context.ResolvedRoute.Description != null))
				path = context.ResolvedRoute.Description.Path ?? path;

			var hitTracker = new HitTracker(HitTrackerSettings.Instance);

			hitTracker.AppendHit(path, new Hit
			{
				StartTime = maybeStarted.HasValue ? maybeStarted.Value : DateTime.MinValue,
				IsError = IsServiceFault(statusCode ?? 500),
				TimeTaken = maybeTimer.HasValue ? maybeTimer.Value.Elapsed : TimeSpan.Zero
			});			
		}

		private static bool IsServiceFault(int httpStatusCode)
		{
			return !((httpStatusCode >= 200 && httpStatusCode <= 299)
				|| (httpStatusCode >= 400 && httpStatusCode <= 499));
		}

		private static Maybe<T> MaybeGetItem<T>(string key, NancyContext context)
		{
			object item;
			if (context.Items.TryGetValue(key, out item))
			{
				context.Items.Remove(key);
				return new Maybe<T>((T)item);
			}

			return Maybe<T>.Empty;
		}

		private static void SetOtHeaderProperties(IDictionary<string, object> properties, RequestHeaders headers)
		{
			if (headers == null)
				return;

			SetFieldFromHeaderValue(properties, "ot-requestid", headers, "ot-requestid");
			SetFieldFromHeaderValue(properties, "ot-userid", headers, "ot-userid");
			SetFieldFromHeaderValue(properties, "ot-sessionid", headers, "ot-sessionid");
			SetFieldFromHeaderValue(properties, "ot-referringhost", headers, "ot-referringhost");
			SetFieldFromHeaderValue(properties, "ot-referringservice", headers, "ot-referringservice");
			SetFieldFromHeaderValue(properties, "ot-domain", headers, "ot-domain");
			SetFieldFromHeaderValue(properties, "ot-clienttime", headers, "ot-clienttime");
		}

		private static void SetFieldFromHeaderValue(IDictionary<string, object> properties,
			string logKey, RequestHeaders headers, string headerName)
		{
			var values = headers[headerName];
			if (values.Any())
			{
				properties[logKey] = values.First();
			}
		}
	}
}