namespace ModularMonolith.Template.Api.Exceptions;

public static class ExceptionHandlerExtensions
{
    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseCustomExceptionHandler()
        {
            ExceptionHandlerOptions options = new()
            {
                StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
                    ? badRequest.StatusCode
                    : StatusCodes.Status500InternalServerError
            };
            builder.UseExceptionHandler(options);
            return builder;
        }
    }
}
