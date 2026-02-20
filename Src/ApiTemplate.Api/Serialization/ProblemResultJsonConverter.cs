using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTemplate.Application.Results;

namespace ApiTemplate.Api.Serialization;

/// <summary>
/// Serializes <see cref="ProblemResult"/> with extension members at the root level
/// (no "extensions" wrapper), so the response has type, title, status and extension data as siblings.
/// </summary>
public sealed class ProblemResultJsonConverter : JsonConverter<ProblemResult>
{
	public override ProblemResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		throw new NotSupportedException("ProblemResult is used for response serialization only.");

	public override void Write(Utf8JsonWriter writer, ProblemResult value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("type", value.Type);
		writer.WriteString("title", value.Title);
		writer.WriteNumber("status", value.Status);

		foreach (var kv in value.Extensions)
		{
			writer.WritePropertyName(kv.Key);
			if (kv.Value is null)
				writer.WriteNullValue();
			else
				JsonSerializer.Serialize(writer, kv.Value, options);
		}

		writer.WriteEndObject();
	}
}
