using EasyApiProxys.HawkAuths;
using HawkNet;

namespace EasyApiProxys
{
    /// <summary>
    /// 用戶端啟用Hawk驗證
    /// </summary>
    public static class HawkAuthExtension
    {
        /// <summary>
        /// 用戶端啟用Hawk驗證
        /// </summary>
        /// <param name="builder">ProxyBuilder</param>
        /// <param name="credential">Hawk憑證</param>
        static public ApiProxyBuilder UseHawkAuthorize(this ApiProxyBuilder builder, HawkCredential credential)
        {
            var hander = new MethodHandler(credential, builder.Options.GetHttpMessageHandler);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        internal class MethodHandler(
            HawkCredential credential,
            Func<HttpMessageHandler> current)
        {
            public HttpMessageHandler GetHandler()
            {
                return new HawkClientMessageHandler(current.Invoke(), credential);
            }
        }
    }
}
