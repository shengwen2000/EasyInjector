using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EasyApiProxys.WebApis
{
    /// <summary>
    /// 預設的API回應封裝
    /// </summary>
    public class DefaultApiResultAttribute : ActionFilterAttribute
    {
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
                    context.Result = new ObjectResult(new DefaultApiResult
                    {
                        Result = "IM",
                        Message = e1.Message
                    });
                }
                else if (ex is ApiCodeException e2)
                {
                    context.Result = new ObjectResult(new DefaultApiResult
                    {
                        Result = e2.Code,
                        Message = e2.Message
                    });
                }
                else
                {
                    context.Result = new ObjectResult(new DefaultApiResult
                    {
                        Result = "EX",
                        Message = ex.Message
                    });
                }
            }
            // 執行正常
            else
            {
                // 有內容
                if (context.Result is ObjectResult robj)
                {
                    context.Result = new ObjectResult(new DefaultApiResult
                    {
                        Result = "OK",
                        Message = "Success",
                        Data = robj.Value
                    });

                }
                // 沒有內容
                else if (context.Result is EmptyResult)
                {
                    context.Result = new ObjectResult(new DefaultApiResult
                    {
                        Result = "OK",
                        Message = "Success"
                    });
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
                        propError = string.Join(",", x.Value!.Errors.Select(y => y.ErrorMessage))
                    })
                    .Select(x => new JsonObject
                    {
                        [x.propName[..1].ToLower() + x.propName[1..]] = x.propError
                    });

                context.Result = new ObjectResult(new DefaultApiResult
                {
                    Result = "IM",
                    Message = "Model State Error",
                    Data = JsonSerializer.Serialize(errors, DefaultApiExtension.DefaultJsonOptions)
                });
            }
        }
    }
}