
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyApiProxys;
using EasyApiProxys.WebApis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 提供DefaultApi協定必要的設定支援
/// </summary>
public static class DefaultApiMvcExtension {

    /// <summary>
    /// 使用DefualtApi協定 Json標準 驼峰命名 日期(無時區與毫秒) 2024-08-06T15:18:41 Enum 一律小寫字串
    /// </summary>
    public static IMvcBuilder AddDefaultApiJsonOptions(this IMvcBuilder builder) {

        builder.AddJsonOptions(opt => {
            opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            opt.JsonSerializerOptions.Converters.Add(new DefaultApiExtension.DateTimeConverter());
            opt.JsonSerializerOptions.Converters.Add(new JsonLowerCaseEnumFactory());
        });
        return builder;
    }
}