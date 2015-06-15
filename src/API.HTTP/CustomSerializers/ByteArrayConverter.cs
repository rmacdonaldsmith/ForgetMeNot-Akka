using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ForgetMeNot.API.HTTP.CustomSerializers
{
	public class ByteArrayConverter : JsonConverter
	{
		public override void WriteJson(
			JsonWriter writer,
			object value,
			JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			byte[] data = (byte[])value;

			writer.WriteRawValue (Encoding.UTF8.GetString (data));
			//writer.WriteRawValue (Convert.ToBase64String( Encoding.UTF8.GetString (data));
		}

		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.StartArray)
			{
				var byteList = new List<byte>();

				while (reader.Read())
				{
					switch (reader.TokenType)
					{
					case JsonToken.Integer:
						//hmmm, what do i need to do here to get UTF8 encoded byte array?
						byteList.Add(Convert.ToByte(reader.Value));
						break;
					case JsonToken.EndArray:
						return byteList.ToArray();
					case JsonToken.Comment:
						// skip
						break;
					default:
						throw new Exception(
							string.Format(
								"Unexpected token when reading bytes: {0}",
								reader.TokenType));
					}
				}

				throw new Exception("Unexpected end when reading bytes.");
			}
			else
			{
				throw new Exception(
					string.Format(
						"Unexpected token parsing binary. "
						+ "Expected StartArray, got {0}.",
						reader.TokenType));
			}
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(byte[]);
		}
	}
}

