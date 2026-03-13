using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SocietyVaccinations
{
    public class ResponseWrapper: ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Result is ObjectResult objResult)
            {
                ApiResponse<object> response;
                if(objResult.StatusCode < 300)
                {
                    response = ApiResponse<object>.Success(
                        objResult.Value,
                        "Success");
                } else
                {
                    response = ApiResponse<object>.Error(objResult?.Value?.GetType().GetProperty("message")?.GetValue(objResult.Value)?.ToString() ?? "?", objResult.StatusCode ?? -1);
                }
                context.Result = new ObjectResult(response)
                {
                    StatusCode = objResult.StatusCode,
                };
            } else if(context.Result is StatusCodeResult statCode)
            {
                var response = ApiResponse<object>.Success(
                    null,
                    "Success");
                context.Result = new ObjectResult(response)
                {
                    StatusCode = statCode.StatusCode,
                };
            }
        }
    }
}
