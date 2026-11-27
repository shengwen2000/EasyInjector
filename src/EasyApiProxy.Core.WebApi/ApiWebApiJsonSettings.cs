using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EasyApiProxys.WebApis;

/// <summary>
/// Web API 2 JSON 序列化設定輔助類別 (對應 DefaultApi / KmuhomeApi 協議)
/// </summary>
public static class ApiWebApiJsonSettings
{
    /// <summary>
    /// DefaultApi 協議預設 JSON 設定 (CamelCase、yyyy-MM-ddTHH:mm:ss、Enum 原始字串)
    /// </summary>
    public static JsonSerializerSettings DefaultApi { get; }

    /// <summary>
    /// KmuhomeApi 協議預設 JSON 設定 (CamelCase、yyyy-MM-ddTHH:mm:ss、Enum 小寫字串)
    /// </summary>
    public static JsonSerializerSettings KmuhomeApi { get; }

    static ApiWebApiJsonSettings()
    {
        DefaultApi = CreateBaseSettings();
        DefaultApi.Converters.Add(new StringEnumConverter());

        KmuhomeApi = CreateBaseSettings();
        KmuhomeApi.Converters.Add(new StringEnumConverter(new LowercaseNamingStrategy()));
    }

    private static JsonSerializerSettings CreateBaseSettings()
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        settings.Converters.Add(new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss"
        });
        return settings;
    }

    private sealed class LowercaseNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name) => name.ToLowerInvariant();
    }
}
