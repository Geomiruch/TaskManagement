using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskManagement
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateTimeString = reader.GetString();
                if (DateTime.TryParse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    return dateTime;
                }

                throw new JsonException($"Unable to convert \"{dateTimeString}\" to System.DateTime.");
            }

            throw new JsonException($"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
