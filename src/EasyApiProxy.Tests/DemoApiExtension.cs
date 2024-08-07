using EasyApiProxys;
using HawkNet;
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
            this ApiProxyBuilder builder, 
            HawkCredential hawkCredential=null)
        {
            var hander = new MethodHandler(builder.Options.GetJsonSerializer, hawkCredential);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        public class MethodHandler
        {
            readonly private Func<JsonSerializer> _getJsonSerializer;
            readonly private HawkCredential _hawkCredential;

            public MethodHandler(               
                Func<JsonSerializer> getJsonSerializer,
                HawkCredential hawkCredential)
            {               
                _getJsonSerializer = getJsonSerializer;
                _hawkCredential = hawkCredential;
            }

            public HttpMessageHandler GetHandler()
            {
                var handler0 = new DemoApiServerMockHandler(_getJsonSerializer);

                var handler1 = new DefaultApiResultHandler(handler0, _hawkCredential);

                return handler1;
            }
        }
    }
}
