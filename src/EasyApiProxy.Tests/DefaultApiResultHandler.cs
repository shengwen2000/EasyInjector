using EasyApiProxys;
using HawkNet;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// 預設的API回應封裝
    /// </summary>
    public class DefaultApiResultHandler : DelegatingHandler
    {
        private readonly HawkCredential _hawkCredential;

        public DefaultApiResultHandler(HttpMessageHandler innerHandler, HawkCredential credential)
            : base(innerHandler)
        {
            _hawkCredential = credential;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {            
            try
            {
                ValidateHawk(request, _hawkCredential);

                var ret = await base.SendAsync(request, cancellationToken);

                // 有內容
                if (ret.Content is ObjectContent)
                {
                    var robj = ret.Content as ObjectContent;
                    ret.Content = new ObjectContent<DefaultApiResult<object>>(new DefaultApiResult<object>
                    {
                        Result = "OK",
                        Message = "Success",
                        Data = robj.Value
                    }, new JsonMediaTypeFormatter());

                }
                // 沒有內容
                else if (ret.Content == null)
                {
                    ret.StatusCode = System.Net.HttpStatusCode.OK;
                    ret.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult
                    {
                        Result = "OK",
                        Message = "Success"
                    }, new JsonMediaTypeFormatter());
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (ex is ValidationException)
                {
                    var e1 = ex as ValidationException;
                    var a = new JsonMediaTypeFormatter();
                    var c = new ObjectContent<DefaultApiResult>(new DefaultApiResult
                    {
                        Result = "IM",
                        Message = e1.Message
                    }, new JsonMediaTypeFormatter());

                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = c;
                    return resp;
                }
                else if (ex is ApiCodeException)
                {
                    var e2 = ex as ApiCodeException;
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult
                    {
                        Result = e2.Code,
                        Message = e2.Message
                    }, new JsonMediaTypeFormatter());
                    return resp;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult
                    {
                        Result = "EX",
                        Message = ex.Message
                    }, new JsonMediaTypeFormatter());
                    return resp;
                }
            }
            
        }

        private void ValidateHawk(HttpRequestMessage request, HawkCredential hawkCredential)
        {
            if (hawkCredential == null) return;          
            if (request.Headers.Authorization == null || request.Headers.Authorization.Parameter == null)
                throw new ApiCodeException("HAWK_FAIL", "HAWK 驗證失敗");    
            var authoriztion = request.Headers.Authorization.Parameter;
            var p = Hawk.Authenticate(authoriztion,
                request.RequestUri.Host,
                request.Method.ToString(),
                request.RequestUri,
                (id) => hawkCredential);
            if (p == null || p.Identity.IsAuthenticated == false)
                throw new ApiCodeException("HAWK_FAIL", "HAWK 驗證失敗");           
        }
    }
}