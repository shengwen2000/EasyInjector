
using EasyApiProxys.NormalAuth;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
namespace EasyApiProxys
{
    /// <summary>
    /// 用戶端啟用一般驗證 
    /// - 自行指定 Http Header Authorization 內容
    /// </summary>
    public static class NormalAuthExtension
    {
        /// <summary>
        /// 用戶端啟用一般驗證
        /// - 自行指定 Http Header Authorization 內容
        /// </summary>
        /// <param name="builder">ProxyBuilder</param>
        /// <param name="authValue">指定 Http Header Authorization 內容 e.g. Basic QVBJOjg1MTU2MTE0MzE1MTYxNjg=</param>
        static public ApiProxyBuilder UseAuthorizationHeader(this ApiProxyBuilder builder, string authValue)
        {
            var hander = new MethodHandler(authValue, builder.Options.GetHttpMessageHandler);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        internal class MethodHandler
        {
            readonly private Func<HttpMessageHandler> _current;
            readonly private AuthenticationHeaderValue _authValue;

            public MethodHandler(
                string authValue,
                Func<HttpMessageHandler> current)
            {
                _current = current;
                _authValue = AuthenticationHeaderValue.Parse(authValue);
            }

            public HttpMessageHandler GetHandler()
            {
                return new NormalClientMessageHandler(_current.Invoke(), _authValue);
            }
        }
    }
}
