using System.Text.Json.Serialization;
using System.Text.Json;
using CommonBackend.Domain.Enums;

namespace CommonBackend.Application.Dtos
{
    public class RabbitMessageDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public required string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public required string To { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        [JsonConverter(typeof(DateOnlyConverter))]
        public required DateOnly Date { get; set; }

        [JsonPropertyName("time")]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public required TimeOnly Time { get; set; }

        [JsonPropertyName("typeMessage")]
        public required TypeMessage TypeMessage { get; set; }
    }

    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateStr = reader.GetString();
            if (dateStr == null)
            {
                throw new JsonException("Expected date string but got null.");
            }
            return DateOnly.ParseExact(dateStr, "yyyyMMdd");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyyMMdd"));
        }
    }

    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeStr = reader.GetString();
            if (timeStr == null)
            {
                throw new JsonException("Expected time string but got null.");
            }
            return TimeOnly.ParseExact(timeStr, "HHmmss");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HHmmss"));
        }
    }
}
