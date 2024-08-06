using HawkNet;
using HawkNet.WebApi;
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
            var hander = new MethodHandler(credential, builder.Options.GetHttpMessageHandler);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;         
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
                HttpMessageHandler base1 = null;
                if (_current != null)
                    base1 = _current.Invoke();

                return new HawkClientMessageHandler(base1, _credential);
            }
        }
    }
}
