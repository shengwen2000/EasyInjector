using HawkNet;
using KmuApps.ApiProxys.Filters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using HawkNet.WebApi;
namespace KmuApps.ApiProxys
{
    public static class HawkAuthExtension
    {
        /// <summary>
        /// 用戶端使用Hawk驗證
        /// </summary>
        /// <param name="builder"></param>
        static public ApiProxyBuilder UseHawkAuthorize(this ApiProxyBuilder builder, HawkCredential credential)
        {
            var hander = new MethodHandler(credential, builder.Options.GetHttpMessageHandler);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;         
            return builder;
        }

        public class MethodHandler
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
