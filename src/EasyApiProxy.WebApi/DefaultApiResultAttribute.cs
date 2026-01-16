using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EasyApiProxys.WebApis
{
    /// <summary>
    /// 預設的API回應封裝
    /// </summary>
    public class DefaultApiResultAttribute : ActionFilterAttribute
    {
        private const string ResultHeader = DefaultApiExtension.HeaderName_Result;

        /// <summary>
        /// 資料類型 {data}
        /// </summary>
        private const string DataTypeHeader = DefaultApiExtension.HeaderName_DataType;
        private const string RESULT_OK = "OK";
        private const string RESULT_EX = "EX";
        private const string RESULT_IM = "IM";

        /// <summary>
        /// 預設發生系統異常(EX)時的 Http 狀態碼，設定為 0 (預設值) 表示不特別指定，維持原本狀態碼。
        /// </summary>
        public int ExStatusCode { get; set; }

        /// <summary>
        /// 預設發生驗證錯誤(IM)時的 Http 狀態碼，設定為 0 (預設值) 表示不特別指定，維持原本狀態碼。
        /// </summary>
        public int ImStatusCode { get; set; }

        public override void OnActionExecuting(HttpActionContext context)
        {
            // Model 檢查
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Any())
                    .Select(x => new
                    {
                        propName = x.Key,
                        propError = string.Join("\n", x.Value.Errors.Select(y => y.ErrorMessage))
                    })
                    .Select(x =>
                    {
                        var jo = new JObject();
                        // "req.Account" => "Account"
                        var pos1 = x.propName.IndexOf('.');
                        if (pos1 > 0)
                            jo[x.propName.Substring(pos1+1)] = x.propError;
                        else
                            jo[x.propName] = x.propError;
                        return jo;
                    });

                var errorsArray = new JArray();
                foreach (var err in errors)
                    errorsArray.Add(err);

                var ret = new DefaultApiResult<JArray>
                {
                    Result = RESULT_IM,
                    Message = "Model State Error",
                    Data = errorsArray
                };

                var jsonMediaTypeFormater = context.ControllerContext.Configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();

                var statusCode = ImStatusCode > 0 ? (HttpStatusCode)ImStatusCode : HttpStatusCode.OK;
                var response1 = new HttpResponseMessage(statusCode)
                {
                    Content = new ObjectContent<DefaultApiResult>(ret, jsonMediaTypeFormater)
                };
                response1.Headers.Add(ResultHeader, ret.Result);
                response1.Headers.Add(DataTypeHeader, GetTypeHint(ret.Data.GetType()));
                context.Response = response1;     
            }
            else
                base.OnActionExecuting(context);
        }

        /// <summary>
        /// Action執行完成 封裝格式
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            var jsonMediaTypeFormater = context.ActionContext.ControllerContext.Configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();

            // 執行過程有異常 回傳格式處理
            if (context.Exception != null)
            {
                context.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

                var ex = context.Exception;
                context.Exception = null;

                if (ex is ValidationException)
                {
                    var e1 = ex as ValidationException;
                    var array1 = new JArray();
                    var err1 = new JObject();
                    var fieldName = string.Join(",", e1.ValidationResult.MemberNames);
                    if (fieldName.Length == 0)
                        fieldName = "$";
                    err1[fieldName] = e1.ValidationResult.ErrorMessage;
                    array1.Add(err1);
                    var ret = new DefaultApiResult
                    {
                        Result = RESULT_IM,
                        Message = "Model State Error",
                        Data = array1
                    };
                    context.Response.Content = new ObjectContent<DefaultApiResult>(ret, jsonMediaTypeFormater);
                    if (ImStatusCode > 0)
                        context.Response.StatusCode = (HttpStatusCode)ImStatusCode;

                    context.Response.Headers.Add(ResultHeader, ret.Result);
                    context.Response.Headers.Add(DataTypeHeader, GetTypeHint(ret.Data.GetType()));
                }
                else if (ex is ApiCodeException)
                {
                    var e2 = ex as ApiCodeException;
                    var ret = new DefaultApiResult
                    {
                        Result = e2.Code,
                        Message = e2.Message,
                        Data = e2.ErrorData
                    };
                    context.Response.Content = new ObjectContent<DefaultApiResult>(ret, jsonMediaTypeFormater);
                    // 1.設定狀態碼
                    if (e2.StatusCode.HasValue)
                        context.Response.StatusCode = (HttpStatusCode)e2.StatusCode.Value;
                    else if (e2.IsSystemError && ExStatusCode > 0)
                        context.Response.StatusCode = (HttpStatusCode)ExStatusCode;
                    else if (e2.IsValidationError && ImStatusCode > 0)
                        context.Response.StatusCode = (HttpStatusCode)ImStatusCode;
                    // 2.設定 Trace ID
                    if (e2.TraceId != null)
                        context.Response.Headers.Add("X-Trace-Id", e2.TraceId);
                    
                    context.Response.Headers.Add(ResultHeader, ret.Result);
                    if (ret.Data != null)
                        context.Response.Headers.Add(DataTypeHeader, GetTypeHint(ret.Data.GetType()));
                }
                else
                {
                    var ret = new DefaultApiResult
                    {
                        Result = RESULT_EX,
                        Message = ex.Message
                    };
                    context.Response.Content = new ObjectContent<DefaultApiResult>(ret, jsonMediaTypeFormater);
                    if (ExStatusCode > 0)
                        context.Response.StatusCode = (HttpStatusCode)ExStatusCode;

                    context.Response.Headers.Add(ResultHeader, ret.Result);
                }
            }
            // 執行正常
            else
            {
                // 有內容
                if (context.Response.Content is ObjectContent)
                {
                    var robj = context.Response.Content as ObjectContent;

                    // 有數值回傳
                    if (robj.Value != null)
                    {
                        context.Response.Headers.Add(ResultHeader, RESULT_OK);
                        context.Response.Headers.Add(DataTypeHeader, GetTypeHint(robj.Value.GetType()));
                    }
                    // 沒有數值
                    else
                    {
                        context.Response.Headers.Add(ResultHeader, RESULT_OK);
                        context.Response.Content = null;
                    }
                }
                // 沒有內容
                else if (context.Response.Content == null)
                {
                    context.Response.Headers.Add(ResultHeader, RESULT_OK);
                }
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// 取得Type名稱簡單的提示，用來讓呼叫者知道是哪種數據
        /// e.g. IEnumerables  Member ...
        /// </summary>
        static string GetTypeHint(Type type)
        {
            if (type == null) return null;

            //匿名型別
            var isAnonymousType = type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                && type.Name.Contains("AnonymousType")
                && (type.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic;
            if (isAnonymousType)
                return "AnonymousType";

            if (type.IsGenericType)
            {
                using (var sw = new StringWriter())
                {
                    var pos = type.Name.IndexOf('`');
                    sw.Write(type.Name.Substring(0, pos));
                    sw.Write("<");

                    var targs = type.GenericTypeArguments;
                    var index = 0;
                    foreach (var targ in targs)
                    {
                        var typeName1 = GetTypeHint(targ);
                        if (index > 0)
                            sw.Write(",");
                        sw.Write(typeName1);
                        index++;
                    }
                    sw.Write(">");
                    return sw.ToString();
                }

            }
            else
            {
                return type.Name;
            }
        }
    }
}