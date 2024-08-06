using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;

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
                    var c = new ObjectContent<DefaultApiResult>(new DefaultApiResult{
                        Result = "IM",
                        Message = e1.Message
                    }, jsonMediaTypeFormater);

                    context.Response.Content = c;
                }
                else if (ex is ApiCodeException)
                {
                    var e2 = ex as ApiCodeException;
                    context.Response.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult{
                        Result = e2.Code,
                        Message = e2.Message
                    }, jsonMediaTypeFormater);
                }
                else
                {
                    context.Response.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult{
                        Result = "EX",
                        Message = ex.Message
                    }, jsonMediaTypeFormater);
                }
            }
            // 執行正常
            else
            {
                
                // 有內容
                if (context.Response.Content is ObjectContent)
                {
                    var robj = context.Response.Content as ObjectContent;

                    context.Response.Content = new ObjectContent<DefaultApiResult<object>>(new DefaultApiResult<object>
                    {
                        Result = "OK",
                        Message = "Success",
                        Data = robj.Value
                    }, jsonMediaTypeFormater);
                   
                }
                // 沒有內容
                else if (context.Response.Content == null)
                {
                    context.Response.StatusCode = System.Net.HttpStatusCode.OK;
                    context.Response.Content = new ObjectContent<DefaultApiResult>(new DefaultApiResult
                    {
                        Result = "OK",
                        Message = "Success"
                    }, jsonMediaTypeFormater);
                }
            }

            base.OnActionExecuted(context);
        }        
    }
}