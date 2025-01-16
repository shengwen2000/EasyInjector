using EasyApiProxys;
using HawkNet;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests
{
    /// <summary>
    /// 預設的API回應封裝
    /// - 測試用
    /// </summary>
    public class DefaultApiResultHandler(HttpMessageHandler innerHandler, HawkCredential? credential) : DelegatingHandler(innerHandler)
    {
        private const string ResultHeader = "X_Api_Result";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (credential != null)
                    ValidateHawk(request, credential);

                var ret = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                // 有內容
                if (ret.Content is JsonContent)
                {
                    var robj = ret.Content as JsonContent;
                    ret.Content = JsonContent.Create(new DefaultApiErrorResult<object>
                    {
                        Result = "OK",
                        Message = "Success",
                        Data = robj?.Value
                    });

                    //System.Net.Http.Em


                }
                // 沒有內容
                else if (ret.Content == null || ret.Content.GetType().Name == "EmptyContent")
                {
                    ret.StatusCode = HttpStatusCode.OK;
                    ret.Content = JsonContent.Create(new DefaultApiErrorResult
                    {
                        Result = "OK",
                        Message = "Success"
                    });
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (ex is ValidationException e1)
                {
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<JsonNode>(e1.Message, DefaultApiExtension.DefaultJsonOptions);

                        var c = JsonContent.Create(new DefaultApiErrorResult<JsonNode>
                        {
                            Result = "IM",
                            Message = "validate error",
                            Data = errorJson
                        });

                        var resp = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = c
                        };
                        return resp;
                    }
                    catch (JsonException)
                    {
                        var c = JsonContent.Create(new DefaultApiErrorResult<string>
                        {
                            Result = "IM",
                            Message = "validate error",
                            Data = e1.Message
                        });
                        var resp = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = c
                        };
                        return resp;
                    }
                }
                else if (ex is ApiCodeException e2)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new DefaultApiErrorResult
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
                        Content = JsonContent.Create(new DefaultApiErrorResult
                        {
                            Result = "EX",
                            Message = ex.Message
                        })
                    };
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
                request.RequestUri!.Host,
                request.Method.ToString(),
                request.RequestUri,
                (id) => hawkCredential);
            if (p == null || p.Identity == null || p.Identity.IsAuthenticated == false)
                throw new ApiCodeException("HAWK_FAIL", "HAWK 驗證失敗");
        }
    }
}