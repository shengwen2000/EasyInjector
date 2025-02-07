using EasyApiProxys.HawkAuths;
using HawkNet;
using System;
using System.Net.Http;

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
            var handler = new MethodHandler(credential, builder.Options.GetHttpMessageHandler);
            builder.Options.Handlers.Add(handler);
            builder.Options.GetHttpMessageHandler = handler.GetHandler;            

            return builder;
        }

        internal class MethodHandler
        {
            readonly private Func<HttpMessageHandler> _current;
            readonly private HawkCredential _credential;            

            public MethodHandler(
                HawkCredential credential,
                Func<HttpMessageHandler> current)              
            {
                _current = current;
                _credential = credential;
            }

            public HttpMessageHandler GetHandler()
            {
                return new HawkClientMessageHandler(_current.Invoke(), _credential);
            }
        }
    }
}
