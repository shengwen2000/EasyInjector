
#pragma warning disable IDE0130 // Namespace does not match folder structure
using EasyApiProxys.BasicAuth;

namespace EasyApiProxys;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// 用戶端啟用Basic驗證
/// </summary>
public static class BasicAuthExtension
{
    /// <summary>
    /// 用戶端啟用Basic驗證
    /// </summary>
    /// <param name="builder">ProxyBuilder</param>
    /// <param name="credential">Basic憑證</param>
    static public ApiProxyBuilder UseBasicAuthorize(this ApiProxyBuilder builder, BasicCredential credential)
    {
        var hander = new MethodHandler(credential, builder.Options.GetHttpMessageHandler);
        builder.Options.GetHttpMessageHandler = hander.GetHandler;
        return builder;
    }

    internal class MethodHandler(
        BasicCredential credential,
        Func<HttpMessageHandler> current)
    {
        public HttpMessageHandler GetHandler()
        {
            return new BasicClientMessageHandler(current.Invoke(), credential);
        }
    }
}
