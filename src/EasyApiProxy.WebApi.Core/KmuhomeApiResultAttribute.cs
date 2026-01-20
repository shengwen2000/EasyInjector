using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Nodes;

namespace EasyApiProxys.WebApis
{
    /// <summary>
    /// KmuhomeAPI回應封裝
    /// </summary>
    public class KmuhomeApiResultAttribute : ActionFilterAttribute
    {
        private const string RESULT_OK = KmuhomeApiConstants.Code_OK;
        private const string RESULT_EX = KmuhomeApiConstants.Code_EX;
        private const string RESULT_IM = KmuhomeApiConstants.Code_IM;

        /// <summary>
        /// 預設發生系統異常(EX)時的 Http 狀態碼，設定為 0 (預設值) 表示不特別指定，維持原本狀態碼。
        /// </summary>
        public int ExStatusCode { get; set; }

        /// <summary>
        /// 預設發生驗證錯誤(IM)時的 Http 狀態碼，設定為 0 (預設值) 表示不特別指定，維持原本狀態碼。
        /// </summary>
        public int ImStatusCode { get; set; }

        /// <summary>
        /// 全域預設發生系統異常(EX)時的 Http 狀態碼
        /// </summary>
        public static int DefaultExStatusCode { get; set; }

        /// <summary>
        /// 全域預設發生驗證錯誤(IM)時的 Http 狀態碼
        /// </summary>
        public static int DefaultImStatusCode { get; set; }

        /// <summary>
        /// 是否向後相容輸出舊版底線 Header (預設 false)
        /// </summary>
        public static bool CompatibleLegacyHeader { get; set; } = false;

        /// <summary>
        /// 全域異常對應表 (方案 A)
        /// </summary>
        public static Dictionary<Type, int> ExceptionMap { get; } = new();

        /// <summary>
        /// Action執行完成 封裝格式
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 執行過程有異常 回傳格式處理
            if (context.Exception != null)
            {
                var ex = context.Exception;
                // 嘗試取得對應的狀態碼
                var mappedStatusCode = GetMappedStatusCode(context);
                context.Exception = null;

                if (ex is ValidationException e1)
                {
                    var array1 = new JsonArray();
                    var err1 = new JsonObject();
                    var fieldName = string.Join(',', e1.ValidationResult.MemberNames);
                    if (fieldName.Length == 0)
                        fieldName = "$";
                    err1[fieldName] = e1.ValidationResult.ErrorMessage;
                    array1.Add(err1);
                    var ret = new DefaultApiResult<JsonNode>
                    {
                        Result = RESULT_IM,
                        Message = "Model State Error",
                        Data = array1
                    };
                    var objResult = new ObjectResult(ret);
                    if (mappedStatusCode.HasValue)
                        objResult.StatusCode = mappedStatusCode.Value;
                    else if (ImStatusCode > 0)
                        objResult.StatusCode = ImStatusCode;
                    else if (DefaultImStatusCode > 0)
                        objResult.StatusCode = DefaultImStatusCode;

                    AddHeaders(context.HttpContext.Response.Headers, ret.Result, GetTypeHint(ret.Data.GetType()));
                    context.Result = objResult;
                }
                else if (ex is ApiCodeException e2)
                {
                    var ret = new DefaultApiResult
                    {
                        Result = e2.Code.ToLower(),
                        Message = e2.Message,
                        Data = e2.ErrorData
                    };
                    var objResult = new ObjectResult(ret);
                    // 1.設定狀態碼
                    if (e2.StatusCode.HasValue)
                        objResult.StatusCode = e2.StatusCode.Value;
                    else if (mappedStatusCode.HasValue)
                        objResult.StatusCode = mappedStatusCode.Value;
                    else if (e2.IsSystemError && (ExStatusCode > 0 || DefaultExStatusCode > 0))
                        objResult.StatusCode = ExStatusCode > 0 ? ExStatusCode : DefaultExStatusCode;
                    else if (e2.IsValidationError && (ImStatusCode > 0 || DefaultImStatusCode > 0))
                        objResult.StatusCode = ImStatusCode > 0 ? ImStatusCode : DefaultImStatusCode;

                    // 2.設定 Trace ID
                    if (e2.TraceId != null)
                        context.HttpContext.Response.Headers["X-Trace-Id"] = e2.TraceId;

                    AddHeaders(context.HttpContext.Response.Headers, ret.Result, ret.Data != null ? GetTypeHint(ret.Data.GetType()) : null);
                    context.Result = objResult;
                }
                else
                {
                    var ret = new DefaultApiResult
                    {
                        Result = RESULT_EX,
                        Message = ex.Message
                    };
                    var objResult = new ObjectResult(ret);
                    if (mappedStatusCode.HasValue)
                        objResult.StatusCode = mappedStatusCode.Value;
                    else if (ExStatusCode > 0)
                        objResult.StatusCode = ExStatusCode;
                    else if (DefaultExStatusCode > 0)
                        objResult.StatusCode = DefaultExStatusCode;

                    AddHeaders(context.HttpContext.Response.Headers, ret.Result);
                    context.Result = objResult;
                }
            }
            // 執行正常
            else
            {
                // 有內容
                if (context.Result is ObjectResult robj)
                {
                    // 有數值回傳
                    if (robj.Value != null)
                    {
                        // 字串轉為 JSON
                        if (robj.DeclaredType == typeof(string))
                            context.Result = new JsonResult(robj.Value);

                        AddHeaders(context.HttpContext.Response.Headers, RESULT_OK, robj.Value != null ? GetTypeHint(robj.Value.GetType()) : null);
                    }
                    // 沒有數值
                    else
                    {
                        AddHeaders(context.HttpContext.Response.Headers, RESULT_OK);
                        context.Result = new NoContentResult();
                    }
                }
                // 沒有內容
                else if (context.Result is EmptyResult)
                {
                    AddHeaders(context.HttpContext.Response.Headers, RESULT_OK);
                    context.Result = new NoContentResult();
                }
            }
            base.OnActionExecuted(context);
        }

        /// <summary>
        /// 統一增加 Header
        /// </summary>
        private static void AddHeaders(Microsoft.AspNetCore.Http.IHeaderDictionary headers, string result, string? dataType = null)
        {
            headers[DefaultApiExtension.HeaderName_Result] = result;
            if (dataType != null)
                headers[DefaultApiExtension.HeaderName_DataType] = dataType;

            if (CompatibleLegacyHeader)
            {
                headers[DefaultApiExtension.HeaderName_Result_Legacy] = result;
                if (dataType != null)
                    headers[DefaultApiExtension.HeaderName_DataType_Legacy] = dataType;
            }
        }

        /// <summary>
        /// 非同步執行
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 類別或方法都有標記的話 只能執行一次
            // 有 Ignore 標記的話 不處理
            if (
                context.ActionDescriptor.EndpointMetadata.OfType<KmuhomeApiResultAttribute>().Last() != this ||
                context.ActionDescriptor.EndpointMetadata.Any(x => x is IgnoreApiResultAttribute))
            {
                await next();
                return;
            }
            //取得執行結果
            var executedCtx = await next();
            OnActionExecuted(executedCtx);
        }

        /// <summary>
        /// 傳入的Model格式錯誤的話
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
             // 類別或方法都有標記的話 只能執行一次(就是方法上的)
            if (context.ActionDescriptor.EndpointMetadata.OfType<KmuhomeApiResultAttribute>().Last() != this)
                return;

            // 有 Ignore 標記的話 不處理
            if (context.ActionDescriptor.EndpointMetadata.Any(x => x is IgnoreApiResultAttribute))
                return;

            if (context.ModelState.IsValid == false)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                    .Select(x => new
                    {
                        propName = x.Key,
                        propError = string.Join("\n", x.Value!.Errors.Select(y => y.ErrorMessage))
                    })
                    .Select(x => new JsonObject
                    {
                        [x.propName] = x.propError
                    });

                var errorsArray = new JsonArray();
                foreach (var err in errors)
                    errorsArray.Add(err);

                var ret = new DefaultApiResult<JsonArray>
                {
                    Result = RESULT_IM,
                    Message = "Model State Error",
                    Data = errorsArray
                };

                context.Result = new ObjectResult(ret);
                if (ImStatusCode > 0)
                    ((ObjectResult)context.Result).StatusCode = ImStatusCode;
                else if (DefaultImStatusCode > 0)
                    ((ObjectResult)context.Result).StatusCode = DefaultImStatusCode;

                AddHeaders(context.HttpContext.Response.Headers, ret.Result, ret.Data != null ? GetTypeHint(ret.Data.GetType()) : null);
            }
        }

        /// <summary>
        /// 取得Type名稱簡單的提示，用來讓呼叫者知道是哪種數據
        /// e.g. IEnumerables  Member ...
        /// </summary>
        static string? GetTypeHint(Type? type)
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
                using var sw = new StringWriter();
                var pos = type.Name.IndexOf('`');
                sw.Write(type.Name[..pos]);
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
            else
            {
                return type.Name;
            }
        }

        /// <summary>
        /// 取得對應的 Http 狀態碼 (方案 A + C)
        /// </summary>
        private int? GetMappedStatusCode(ActionExecutedContext context)
        {
            var ex = context.Exception;
            if (ex == null) return null;

            var exType = ex.GetType();

            // 1. 優先從 Action/Controller 特性查找 (方案 C)
            // 由於泛型特性在執行期需要反射處理
            var attr = context.ActionDescriptor.EndpointMetadata
                .LastOrDefault(m =>
                {
                    var t = m.GetType();
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ExceptionStatusAttribute<>))
                    {
                        var targetExType = t.GetProperty(nameof(ExceptionStatusAttribute<Exception>.ExceptionType))?.GetValue(m) as Type;
                        return targetExType == exType;
                    }
                    return false;
                });

            if (attr != null)
            {
                return (int?)attr.GetType().GetProperty(nameof(ExceptionStatusAttribute<Exception>.StatusCode))?.GetValue(attr);
            }

            // 2. 從全域對應表查找 (方案 A)
            if (ExceptionMap.TryGetValue(exType, out int statusCode))
                return statusCode;

            return null;
        }
    }
}