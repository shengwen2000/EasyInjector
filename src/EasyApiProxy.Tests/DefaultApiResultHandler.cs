using KmuApps.ApiProxys;
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
        public DefaultApiResultHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {            
            try
            {
                var ret = await base.SendAsync(request, cancellationToken);

                // 有內容
                if (ret.Content is ObjectContent)
                {
                    var robj = ret.Content as ObjectContent;
                    ret.Content = new ObjectContent<KmuApps.ApiProxys.DefaultApiResult<object>>(new KmuApps.ApiProxys.DefaultApiResult<object>
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
                    ret.Content = new ObjectContent<KmuApps.ApiProxys.DefaultApiResult>(new KmuApps.ApiProxys.DefaultApiResult
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
                    var c = new ObjectContent<KmuApps.ApiProxys.DefaultApiResult>(new KmuApps.ApiProxys.DefaultApiResult
                    {
                        Result = "IM",
                        Message = e1.Message
                    }, new JsonMediaTypeFormatter());

                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = c;
                    return resp;
                }
                else if (ex is DefaultApiCodeError)
                {
                    var e2 = ex as DefaultApiCodeError;
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new ObjectContent<KmuApps.ApiProxys.DefaultApiResult>(new KmuApps.ApiProxys.DefaultApiResult
                    {
                        Result = e2.Code,
                        Message = e2.Message
                    }, new JsonMediaTypeFormatter());
                    return resp;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new ObjectContent<KmuApps.ApiProxys.DefaultApiResult>(new KmuApps.ApiProxys.DefaultApiResult
                    {
                        Result = "EX",
                        Message = ex.Message
                    }, new JsonMediaTypeFormatter());
                    return resp;
                }
            }
            
        }
    }
}