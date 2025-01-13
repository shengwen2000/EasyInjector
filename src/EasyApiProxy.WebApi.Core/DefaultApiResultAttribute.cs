using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Nodes;

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
                        Result = "IM",
                        Message = "Model State Error",
                        Data = array1
                    };
                    context.Result = new ObjectResult(ret);
                    context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                    context.HttpContext.Response.Headers[DataTypeHeader] = ret.Data.GetType().FullName;
                }
                else if (ex is ApiCodeException e2)
                {
                    var ret = new DefaultApiResult
                    {
                        Result = e2.Code,
                        Message = e2.Message,
                        Data = e2.ErrorData
                    };
                    context.Result = new ObjectResult(ret);
                    context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                    if (ret.Data != null)
                        context.HttpContext.Response.Headers[DataTypeHeader] = ret.Data.GetType().FullName;
                }
                else
                {
                    var ret = new DefaultApiResult
                    {
                        Result = "EX",
                        Message = ex.Message
                    };
                    context.Result = new ObjectResult(ret);
                    context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                }
            }
            // 執行正常
            else
            {
                // 有內容
                if (context.Result is ObjectResult robj)
                {
                    var ret = new DefaultApiResult
                    {
                        Result = "OK",
                        Data = robj.Value
                    };
                    context.Result = new ObjectResult(ret);
                    context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                    if (ret.Data != null)
                        context.HttpContext.Response.Headers[DataTypeHeader] = ret.Data.GetType().FullName;
                }
                // 沒有內容
                else if (context.Result is EmptyResult)
                {
                    var ret = new DefaultApiResult
                    {
                        Result = "OK",
                    };

                    context.Result = new ObjectResult(ret);
                    context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                }
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// 非同步執行
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //取得執行結果
            var executedCtx = await next();
            OnActionExecuted(executedCtx);
        }

        /// <summary>
        /// 傳入的Model格式錯誤的話
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
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
                    Result = "IM",
                    Message = "Model State Error",
                    Data = errorsArray
                };

                context.Result = new ObjectResult(ret);
                context.HttpContext.Response.Headers[ResultHeader] = ret.Result;
                if (ret.Data != null)
                    context.HttpContext.Response.Headers[DataTypeHeader] = ret.Data.GetType().FullName;
            }
        }
    }
}