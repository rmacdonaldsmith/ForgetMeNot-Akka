using Newtonsoft.Json;

namespace ForgetMeNot.API.HTTP.CustomSerializers
{
	public class CustomJsonSerializer : JsonSerializer
	{
		public CustomJsonSerializer ()
		{
			//so that enums are serialized to their string value and not the integer value
			this.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

			//so that we can serialize the byte[] as a json string
			this.Converters.Add (new ByteArrayConverter());

		}
	}
}

