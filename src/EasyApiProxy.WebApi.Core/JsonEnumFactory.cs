using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyApiProxys.WebApis;

internal class JsonEnumFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonEnumConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(converterType) as JsonConverter ?? throw new NotSupportedException(typeToConvert.FullName);
    }
}

class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (Enum.TryParse(enumString, true, out T value))
            return value;
        throw new JsonException($"Unable to convert \"{enumString}\" to Enum \"{typeof(T)}\".");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}