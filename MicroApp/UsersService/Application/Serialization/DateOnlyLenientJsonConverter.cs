using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroApp.UsersService.Application.Serialization;

/// <summary>
/// A lenient System.Text.Json converter for DateOnly that accepts either
/// - a plain date string in ISO format (yyyy-MM-dd), or
/// - a full ISO-8601 date-time string (with time/offset), from which the date part is taken.
/// </summary>
public sealed class DateOnlyLenientJsonConverter : JsonConverter<DateOnly>
{
    private static readonly string[] DateFormats = new[]
    {
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "dd.MM.yyyy",
        "MM/dd/yyyy"
    };

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new JsonException("Empty string is not a valid DateOnly value");
            }

            // 1) Try strict DateOnly formats first
            if (DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1))
            {
                return d1;
            }

            // 2) Try parsing as DateTimeOffset/DateTime (ISO-8601) and take the date part
            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
            {
                return DateOnly.FromDateTime(dto.UtcDateTime.Date);
            }
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            {
                return DateOnly.FromDateTime(dt.Date);
            }

            throw new JsonException("The JSON value could not be converted to DateOnly.");
        }

        // Accept numbers as Unix epoch seconds or milliseconds? Not required/useful here -> reject
        throw new JsonException($"Unexpected token parsing DateOnly. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
