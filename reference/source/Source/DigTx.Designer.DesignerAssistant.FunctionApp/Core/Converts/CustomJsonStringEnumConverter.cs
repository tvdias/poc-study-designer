namespace DigTx.Designer.FunctionApp.Core.Converts;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomJsonStringEnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();

            foreach (var name in Enum.GetNames(typeof(T)))
            {
                if (string.Equals(name, raw, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)Enum.Parse(typeof(T), name);
                }
            }

            throw new JsonException($"Invalid value {raw} for enum type {typeof(T).Name}.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), intValue))
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }

            throw new JsonException($"Integer value '{intValue}' is not valid for enum type {typeof(T).Name}.");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum {typeof(T).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
