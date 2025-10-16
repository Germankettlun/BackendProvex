using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProvexBackendAPI.Dto.ApiResponse;

namespace ProvexBackendAPI.Filters
{
    public class ApiResponseWrapperFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
      
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is not ApiResponse<object>)
                {
                    var wrapped = new ApiResponse<object>(objectResult.Value, (int)objectResult.StatusCode);
                    context.Result = new ObjectResult(wrapped)
                    {
                        StatusCode = objectResult.StatusCode,
                    };
                  
                }
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new ApiResponse<object>(null, 204, true, "Sin contenido"))
                {
                    StatusCode = 204
                };
            }
        }
    }
}
