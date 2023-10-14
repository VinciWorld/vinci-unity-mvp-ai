using System;
using Newtonsoft.Json;

public class TrainJobStatusConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Implement this if you need to serialize the enum to JSON.
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string enumValue = (string)reader.Value;

            if (Enum.TryParse(typeof(TrainJobStatus), enumValue, out var status))
            {
                return status;
            }
        }

        return TrainJobStatus.SUBMITTED; // Default value if parsing fails
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TrainJobStatus);
    }
}