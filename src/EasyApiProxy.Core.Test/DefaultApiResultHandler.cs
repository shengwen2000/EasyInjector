using EasyApiProxys;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;

namespace Tests
{
    /// <summary>
    /// 預設的API回應封裝
    /// </summary>
    public class DefaultApiResultHandler : DelegatingHandler
    {

        public DefaultApiResultHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var ret = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                // 有內容
                if (ret.Content is JsonContent)
                {
                    var robj = ret.Content as JsonContent;
                    ret.Content = JsonContent.Create(new DefaultApiResult<object>
                    {
                        Result = "OK",
                        Message = "Success",
                        Data = robj.Value
                    });

                    //System.Net.Http.Em


                }
                // 沒有內容
                else if (ret.Content == null || ret.Content.GetType().Name == "EmptyContent")
                {
                    ret.StatusCode = HttpStatusCode.OK;
                    ret.Content = JsonContent.Create(new DefaultApiResult
                    {
                        Result = "OK",
                        Message = "Success"
                    });
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (ex is ValidationException)
                {
                    var e1 = ex as ValidationException;
                    var c = JsonContent.Create(new DefaultApiResult
                    {
                        Result = "IM",
                        Message = e1.Message
                    });

                    var resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = c
                    };
                    return resp;
                }
                else if (ex is ApiCodeException)
                {
                    var e2 = ex as ApiCodeException;
                    var resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new DefaultApiResult
                        {
                            Result = e2.Code,
                            Message = e2.Message
                        })
                    };
                    return resp;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new DefaultApiResult
                        {
                            Result = "EX",
                            Message = ex.Message
                        })
                    };
                    return resp;
                }
            }
        }
    }
}