using HawkNet.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Hawk擴充 for Add Service
/// </summary>
public static class HawkAuthorizeExtension
{
    /// <summary>
    /// 使用 Hawk 驗證
    /// </summary>
    static public AuthenticationBuilder AddHawkAuthorize(this AuthenticationBuilder builder, Action<HawkAuthorizeOptions>? configureOptions=null)
    {
        builder.Services.AddOptions<HawkAuthorizeOptions>();

        if (configureOptions != null)
            builder.Services.Configure(configureOptions);

        builder.AddScheme<HawkAuthorizeOptions, HawkAuthorizeHanlder>(
            HawkAuthorizeHanlder.AuthenticationScheme, null);
        return builder;
    }
}