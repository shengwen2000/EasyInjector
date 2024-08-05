using KmuApps.ApiProxys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    static class DemoApiExtension
    {
        /// <summary>
        /// 用戶端使用Hawk驗證
        /// </summary>
        /// <param name="builder"></param>
        static public ApiProxyBuilder UseDemoApiServerMock(this ApiProxyBuilder builder)
        {
            var hander = new MethodHandler(builder.Options.GetHttpMessageHandler, builder.Options.GetJsonSerializer);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        public class MethodHandler
        {
            readonly private Func<HttpMessageHandler> _current;
            readonly private Func<JsonSerializer> _current2;

            public MethodHandler(
                Func<HttpMessageHandler> current, 
                Func<JsonSerializer> current2)
            {
                _current = current;
                _current2 = current2;
            }

            public HttpMessageHandler GetHandler()
            {
                HttpMessageHandler base1 = null;
                if (_current != null)
                    base1 = _current.Invoke();

                var handler0 = new DemoApiServerMockHandler(_current2);

                var handler1 = new DefaultApiResultHandler(handler0);

                return handler1;
            }
        }
    }
}
