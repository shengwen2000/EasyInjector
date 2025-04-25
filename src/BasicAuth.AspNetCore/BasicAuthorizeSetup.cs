using BasicAuth.AspNetCore;
using Microsoft.AspNetCore.Authentication;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class BasicAuthorizeSetup
    {
        /// <summary>
        /// Basic 驗證
        /// </summary>
        static public AuthenticationBuilder AddBasicAuthorize(this AuthenticationBuilder builder, Action<BasicAuthorizeOptions>? configureOptions = null)
        {
            builder.Services.AddOptions<BasicAuthorizeOptions>();
            if (configureOptions != null)
                builder.Services.Configure(configureOptions);

            builder.AddScheme<BasicAuthorizeOptions, BasicAuthorizeHanlder>(
                BasicAuthorizeHanlder.AuthenticationScheme, null);
            return builder;
        }
    }
}