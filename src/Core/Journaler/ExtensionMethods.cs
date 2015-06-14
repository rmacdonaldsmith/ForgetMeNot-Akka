using System;
using System.Data;
using System.Web.Script.Serialization;
using ForgetMeNot.Common;

namespace ForgetMeNot.Core.Journaler
{
	public static class ExtensionMethods
	{
		//use automapper?
		//use dapper?
	    public static T Get<T>(this IDataRecord reader, string fieldName, T defaultIfNull = default(T)) where T : class
		{
			var raw = reader [fieldName];

			if (raw is DBNull)
			{
			    return defaultIfNull;
			}

		    return (T)raw;
		}

		public static Maybe<T> MaybeGet<T>(this IDataRecord reader, string fieldName)
		{
			var raw = reader [fieldName];

			if (raw is DBNull)
				return Maybe<T>.Empty;

			return new Maybe<T> ((T)raw);
		}

		//hacky, because postgres know nothing about Guids and stores GUids as strings, so no native support in the driver
		public static Guid GetGuid(this IDataRecord reader, string fieldName)
		{
			var raw = reader [fieldName];

			if (raw is DBNull)
				return Guid.Empty;

			return Guid.ParseExact (raw.ToString(), "D");
		}

		public static string AsJson(this object message)
		{
			var serializer = new JavaScriptSerializer ();
			return serializer.Serialize (message);
		}
	}
}

