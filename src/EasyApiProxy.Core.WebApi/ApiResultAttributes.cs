using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json.Linq;

namespace EasyApiProxys.WebApis;

/// <summary>
/// EasyApiProxy Web API 2 回應封裝基底過濾器 (統一處理 DefaultApi 與 KmuhomeApi)
/// </summary>
public abstract class ApiResultFilterBase : ActionFilterAttribute
{
    internal abstract string CodeOk { get; }
    internal abstract string CodeEx { get; }
    internal abstract string CodeIm { get; }
    internal abstract bool UseLegacyHeader { get; }
    internal abstract int ConfiguredExStatusCode { get; }
    internal abstract int ConfiguredImStatusCode { get; }
    internal abstract Dictionary<Type, int> GlobalExceptionMap { get; }

    /// <summary>
    /// 發生系統異常(EX)時的 Http 狀態碼 (0 表示維持 200)
    /// </summary>
    public int ExStatusCode { get; set; }

    /// <summary>
    /// 發生驗證錯誤(IM)時的 Http 狀態碼 (0 表示維持 200)
    /// </summary>
    public int ImStatusCode { get; set; }

    /// <inheritdoc />
    public override void OnActionExecuting(HttpActionContext context)
    {
        var attr = context.ActionDescriptor.GetCustomAttributes<ApiResultFilterBase>()
            .Union(context.ControllerContext.ControllerDescriptor.GetCustomAttributes<ApiResultFilterBase>())
            .FirstOrDefault();

        if (attr != this)
            return;

        if (context.ActionDescriptor.GetCustomAttributes<IgnoreApiResultAttribute>().Any())
            return;

        if (!context.ModelState.IsValid)
        {
            var errorsArray = new JArray();
            foreach (var item in context.ModelState.Where(x => x.Value.Errors.Any()))
            {
                var jo = new JObject();
                var propName = item.Key;
                var pos = propName.IndexOf('.');
                var key = pos > 0 ? propName.Substring(pos + 1) : propName;
                jo[key] = string.Join("\n", item.Value.Errors.Select(y => string.IsNullOrEmpty(y.ErrorMessage) ? y.Exception?.Message : y.ErrorMessage));
                errorsArray.Add(jo);
            }

            var ret = new DefaultApiResult<JArray>
            {
                Result = CodeIm,
                Message = "Model State Error",
                Data = errorsArray
            };

            var formatter = context.ControllerContext.Configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();
            var statusCode = ImStatusCode > 0
                ? (HttpStatusCode)ImStatusCode
                : (ConfiguredImStatusCode > 0 ? (HttpStatusCode)ConfiguredImStatusCode : HttpStatusCode.OK);

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new ObjectContent<DefaultApiResult>(ret, formatter)
            };
            AddHeaders(response, ret.Result, GetTypeHint(ret.Data.GetType()));
            context.Response = response;
        }
        else
        {
            base.OnActionExecuting(context);
        }
    }

    /// <inheritdoc />
    public override void OnActionExecuted(HttpActionExecutedContext context)
    {
        var attr = context.ActionContext.ActionDescriptor.GetCustomAttributes<ApiResultFilterBase>()
            .Union(context.ActionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<ApiResultFilterBase>())
            .FirstOrDefault();

        if (attr != this)
            return;

        if (context.ActionContext.ActionDescriptor.GetCustomAttributes<IgnoreApiResultAttribute>().Any())
            return;

        var mappedStatusCode = GetMappedStatusCode(context);
        var formatter = context.ActionContext.ControllerContext.Configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();

        if (context.Exception != null)
        {
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            var ex = context.Exception;
            context.Exception = null;

            if (ex is ValidationException e1)
            {
                var array1 = new JArray();
                var err1 = new JObject();
                var fieldName = string.Join(",", e1.ValidationResult.MemberNames);
                if (fieldName.Length == 0) fieldName = "$";
                err1[fieldName] = e1.ValidationResult.ErrorMessage;
                array1.Add(err1);

                var ret = new DefaultApiResult
                {
                    Result = CodeIm,
                    Message = "Model State Error",
                    Data = array1
                };
                context.Response.Content = new ObjectContent<DefaultApiResult>(ret, formatter);
                if (mappedStatusCode.HasValue)
                    context.Response.StatusCode = (HttpStatusCode)mappedStatusCode.Value;
                else if (ImStatusCode > 0)
                    context.Response.StatusCode = (HttpStatusCode)ImStatusCode;
                else if (ConfiguredImStatusCode > 0)
                    context.Response.StatusCode = (HttpStatusCode)ConfiguredImStatusCode;

                AddHeaders(context.Response, ret.Result, GetTypeHint(ret.Data.GetType()));
            }
            else if (ex is ApiCodeException e2)
            {
                var ret = new DefaultApiResult
                {
                    Result = e2.Code,
                    Message = e2.Message,
                    Data = e2.ErrorData
                };
                context.Response.Content = new ObjectContent<DefaultApiResult>(ret, formatter);
                if (e2.StatusCode.HasValue)
                    context.Response.StatusCode = (HttpStatusCode)e2.StatusCode.Value;
                else if (mappedStatusCode.HasValue)
                    context.Response.StatusCode = (HttpStatusCode)mappedStatusCode.Value;
                else if (e2.IsSystemError && (ExStatusCode > 0 || ConfiguredExStatusCode > 0))
                    context.Response.StatusCode = (HttpStatusCode)(ExStatusCode > 0 ? ExStatusCode : ConfiguredExStatusCode);
                else if (e2.IsValidationError && (ImStatusCode > 0 || ConfiguredImStatusCode > 0))
                    context.Response.StatusCode = (HttpStatusCode)(ImStatusCode > 0 ? ImStatusCode : ConfiguredImStatusCode);

                if (e2.TraceId != null)
                    context.Response.Headers.Add("X-Trace-Id", e2.TraceId);

                AddHeaders(context.Response, ret.Result, ret.Data != null ? GetTypeHint(ret.Data.GetType()) : null);
            }
            else
            {
                var ret = new DefaultApiResult
                {
                    Result = CodeEx,
                    Message = ex.Message
                };
                context.Response.Content = new ObjectContent<DefaultApiResult>(ret, formatter);
                if (mappedStatusCode.HasValue)
                    context.Response.StatusCode = (HttpStatusCode)mappedStatusCode.Value;
                else if (ExStatusCode > 0)
                    context.Response.StatusCode = (HttpStatusCode)ExStatusCode;
                else if (ConfiguredExStatusCode > 0)
                    context.Response.StatusCode = (HttpStatusCode)ConfiguredExStatusCode;

                AddHeaders(context.Response, ret.Result);
            }
        }
        else if (context.Response != null)
        {
            if (context.Response.Content is ObjectContent robj)
            {
                if (robj.Value != null)
                {
                    AddHeaders(context.Response, CodeOk, GetTypeHint(robj.Value.GetType()));
                }
                else
                {
                    AddHeaders(context.Response, CodeOk);
                    context.Response.Content = null;
                }
            }
            else if (context.Response.Content == null)
            {
                AddHeaders(context.Response, CodeOk);
            }
        }

        base.OnActionExecuted(context);
    }

    private void AddHeaders(HttpResponseMessage response, string result, string? dataType = null)
    {
        response.Headers.Add(DefaultApiConstants.Header_Result, result);
        if (dataType != null)
            response.Headers.Add(DefaultApiConstants.Header_DataType, dataType);

        var exposedHeaders = DefaultApiConstants.Header_Result;
        if (UseLegacyHeader)
        {
            response.Headers.Add(DefaultApiConstants.Header_Result_Legacy, result);
            if (dataType != null)
                response.Headers.Add(DefaultApiConstants.Header_DataType_Legacy, dataType);
            exposedHeaders += ", " + DefaultApiConstants.Header_Result_Legacy;
        }

        response.Headers.Add("Access-Control-Expose-Headers", exposedHeaders);
    }

    private static string? GetTypeHint(Type? type)
    {
        if (type == null) return null;

        var isAnonymousType = type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
            && type.Name.Contains("AnonymousType")
            && (type.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic;
        if (isAnonymousType)
            return "AnonymousType";

        if (type.IsGenericType)
        {
            using var sw = new StringWriter();
            var pos = type.Name.IndexOf('`');
            sw.Write(type.Name.Substring(0, pos));
            sw.Write("<");
            var targs = type.GetGenericArguments();
            for (var i = 0; i < targs.Length; i++)
            {
                if (i > 0) sw.Write(",");
                sw.Write(GetTypeHint(targs[i]));
            }
            sw.Write(">");
            return sw.ToString();
        }

        return type.Name;
    }

    private int? GetMappedStatusCode(HttpActionExecutedContext context)
    {
        var ex = context.Exception;
        if (ex == null) return null;

        var exType = ex.GetType();
        var actionAttr = context.ActionContext.ActionDescriptor.GetCustomAttributes<ExceptionStatusMapAttribute>()
            .FirstOrDefault(a => a.ExceptionType == exType);
        if (actionAttr != null) return actionAttr.StatusCode;

        var controllerAttr = context.ActionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<ExceptionStatusMapAttribute>()
            .FirstOrDefault(a => a.ExceptionType == exType);
        if (controllerAttr != null) return controllerAttr.StatusCode;

        return GlobalExceptionMap.TryGetValue(exType, out var statusCode) ? statusCode : null;
    }
}

/// <summary>
/// 指定特定 Exception 類型對應的 Http 狀態碼 (.NET Framework Web API 2 版)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ExceptionStatusMapAttribute(Type exceptionType, int statusCode) : Attribute
{
    /// <summary>例外類型</summary>
    public Type ExceptionType { get; } = exceptionType;

    /// <summary>對應 HTTP 狀態碼</summary>
    public int StatusCode { get; } = statusCode;
}

/// <summary>
/// KmuhomeAPI 回應封裝
/// </summary>
public class KmuhomeApiResultAttribute : ApiResultFilterBase
{
    /// <summary>是否向後相容輸出舊版底線 Header (預設 false)</summary>
    public static bool CompatibleLegacyHeader { get; set; }

    /// <summary>全域預設系統異常(EX) Http 狀態碼</summary>
    public static int DefaultExStatusCode { get; set; }

    /// <summary>全域預設驗證錯誤(IM) Http 狀態碼</summary>
    public static int DefaultImStatusCode { get; set; }

    /// <summary>全域異常對應表</summary>
    public static Dictionary<Type, int> ExceptionMap { get; } = [];

    internal override string CodeOk => KmuhomeApiConstants.Code_OK;
    internal override string CodeEx => KmuhomeApiConstants.Code_EX;
    internal override string CodeIm => KmuhomeApiConstants.Code_IM;
    internal override bool UseLegacyHeader => CompatibleLegacyHeader;
    internal override int ConfiguredExStatusCode => DefaultExStatusCode;
    internal override int ConfiguredImStatusCode => DefaultImStatusCode;
    internal override Dictionary<Type, int> GlobalExceptionMap => ExceptionMap;
}

/// <summary>
/// 預設 API 回應封裝 (DefaultApi 協議)
/// </summary>
public class DefaultApiResultAttribute : ApiResultFilterBase
{
    /// <summary>是否向後相容輸出舊版底線 Header (預設 false)</summary>
    public static bool CompatibleLegacyHeader { get; set; }

    /// <summary>全域預設系統異常(EX) Http 狀態碼</summary>
    public static int DefaultExStatusCode { get; set; }

    /// <summary>全域預設驗證錯誤(IM) Http 狀態碼</summary>
    public static int DefaultImStatusCode { get; set; }

    /// <summary>全域異常對應表</summary>
    public static Dictionary<Type, int> ExceptionMap { get; } = [];

    internal override string CodeOk => DefaultApiConstants.Code_OK;
    internal override string CodeEx => DefaultApiConstants.Code_EX;
    internal override string CodeIm => DefaultApiConstants.Code_IM;
    internal override bool UseLegacyHeader => CompatibleLegacyHeader;
    internal override int ConfiguredExStatusCode => DefaultExStatusCode;
    internal override int ConfiguredImStatusCode => DefaultImStatusCode;
    internal override Dictionary<Type, int> GlobalExceptionMap => ExceptionMap;
}
