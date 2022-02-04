using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace MatchNBuy.API.Json;

public class DateTimeConverter : JsonConverter<DateTime>
{
	/// <inheritdoc />
	public override DateTime Read(ref Utf8JsonReader reader, [NotNull] Type typeToConvert, JsonSerializerOptions options)
	{
		if (!typeof(DateTime).IsAssignableFrom(typeToConvert)) throw new InvalidOperationException();
		string value = reader.GetString();
		if (string.IsNullOrEmpty(value)) return DateTime.MinValue;
		return DateTime.Parse(value);
	}

	/// <inheritdoc />
	public override void Write([NotNull] Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd hh':'mm':'ss"));
	}
}