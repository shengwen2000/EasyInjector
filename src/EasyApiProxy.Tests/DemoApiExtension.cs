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
            var hander = new MethodHandler(builder.Options.GetHttpMessageHandler, builder.Options.GetJsonSerializer, hawkCredential);
            builder.Options.GetHttpMessageHandler = hander.GetHandler;
            return builder;
        }

        public class MethodHandler
        {
            readonly private Func<HttpMessageHandler> _current;
            readonly private Func<JsonSerializer> _current2;
            readonly private HawkCredential _hawkCredential;

            public MethodHandler(
                Func<HttpMessageHandler> current, 
                Func<JsonSerializer> current2,
                HawkCredential hawkCredential)
            {
                _current = current;
                _current2 = current2;
                _hawkCredential = hawkCredential;
            }

            public HttpMessageHandler GetHandler()
            {
                HttpMessageHandler base1 = null;
                if (_current != null)
                    base1 = _current.Invoke();

                var handler0 = new DemoApiServerMockHandler(_current2);

                var handler1 = new DefaultApiResultHandler(handler0, _hawkCredential);

                return handler1;
            }
        }
    }
}
