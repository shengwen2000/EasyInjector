
using EasyApiProxys.BasicAuth;
using System;
using System.Net.Http;
namespace EasyApiProxys
{
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

        internal class MethodHandler
        {
            readonly private Func<HttpMessageHandler> _current;
            readonly private BasicCredential _credential;

            public MethodHandler(
                BasicCredential credential,
                Func<HttpMessageHandler> current)
            {
                _current = current;
                _credential = credential;
            }

            public HttpMessageHandler GetHandler()
            {
                return new BasicClientMessageHandler(_current.Invoke(), _credential);
            }
        }
    }
}
