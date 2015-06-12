using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using log4net;

namespace ForgetMeNot.Common
{
	// log4net.ILog extensions make it easy to add arbitrary fields to log entries using syntax like this:
	// log.InfoFormat(new { logname="metrics", duration=elapsed }, "Method execution took {0} microsecs", elapsed);
	public static class LoggingExtensions
	{
		public static void InfoFormat(this ILog logger, object data, string format, params object[] parms)
		{
			var props = CreateDictionaryWithMessage(string.Format(format, parms)).MergeObject(data);
			logger.Info(props);
		}

		public static void WarnFormat(this ILog logger, object data, string format, params object[] parms)
		{
			var props = CreateDictionaryWithMessage(string.Format(format, parms)).MergeObject(data);
			logger.Warn(props);
		}

		public static void ErrorFormat(this ILog logger, Exception ex, object data, string format, params object[] parms)
		{
			var props = CreateDictionaryWithMessage(string.Format(format, parms)).MergeObject(data);
			if (ex == null)
				logger.Error(props);
			else
				logger.Error(props, ex);
		}

		public static void ErrorFormatNoException(this ILog logger, object data, string format, params object[] parms)
		{
			ErrorFormat(logger, null, data, format, parms);
		}

		private static Dictionary<string, object> CreateDictionaryWithMessage(string message)
		{
			return new Dictionary<string, object> { { "logmessage", message } };
		}

		private static IDictionary<string, object> MergeObject(this IDictionary<string, object> dest, object values)
		{
			if (dest == null)
				dest = new Dictionary<string, object>();

			if (values == null)
				return dest;

			foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
			{
				object obj = propertyDescriptor.GetValue(values);
				if (obj != null)
					dest[propertyDescriptor.Name.ToLower().Replace('_', '-')] = obj;
			}

			return dest;
		}
	}
}

