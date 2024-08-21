using EasyApiProxys;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Tests
{
    static class DemoApiExtension
    {
        /// <summary>
        /// 用戶端使用Hawk驗證
        /// </summary>
        /// <param name="builder"></param>
        static public ApiProxyBuilder UseDemoApiServerMock(
            this ApiProxyBuilder builder)
        {
            var hander = new MethodHandler(builder.Options.GetJsonSerializer);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        public class MethodHandler
        {
            readonly private Func<JsonSerializer> _getJsonSerializer;
            //readonly private HawkCredential _hawkCredential;

            public MethodHandler(
                Func<JsonSerializer> getJsonSerializer)
            {
                _getJsonSerializer = getJsonSerializer;
            }

            public HttpMessageHandler GetHandler()
            {
                var handler0 = new DemoApiServerMockHandler(_getJsonSerializer);

                var handler1 = new DefaultApiResultHandler(handler0);

                return handler1;
            }
        }
    }
}
