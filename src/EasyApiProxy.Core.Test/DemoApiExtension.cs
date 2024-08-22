using EasyApiProxys;
using System.Text.Json;
using System;
using System.Net.Http;
using HawkNet;

namespace Tests
{
    static class DemoApiExtension
    {
        /// <summary>
        /// 用戶端使用Hawk驗證
        /// </summary>
        /// <param name="builder"></param>
        static public ApiProxyBuilder UseDemoApiServerMock(
            this ApiProxyBuilder builder, HawkCredential? hawkCredential=null)
        {
            var hander = new MethodHandler(builder.Options.JsonOptions, hawkCredential);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        public class MethodHandler(
            JsonSerializerOptions getJsonSerializer, HawkCredential? hawkCredential)
        {
            public HttpMessageHandler GetHandler()
            {
                var handler0 = new DemoApiServerMockHandler(getJsonSerializer);

                var handler1 = new DefaultApiResultHandler(handler0, hawkCredential);

                return handler1;
            }
        }
    }
}
