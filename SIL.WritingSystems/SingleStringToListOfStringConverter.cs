using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SIL.WritingSystems
{
	internal class SingleStringToListOfStringConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			object retVal = new Object();
			if (reader.TokenType == JsonToken.String)
			{
				var instance = (string)serializer.Deserialize(reader, typeof(string));
				retVal = new List<string> { instance };
			}
			else if (reader.TokenType == JsonToken.StartArray)
			{
				retVal = serializer.Deserialize(reader, objectType);
			}
			return retVal;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(string) || objectType == typeof(List<string>);
		}
	}
}