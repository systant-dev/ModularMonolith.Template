namespace ModularMonolith.Template.Api.Exceptions;

public static class ExceptionHandlerExtensions
{
    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseCustomExceptionHandler()
        {
            ExceptionHandlerOptions options = new()
            {
                StatusCodeSelector = exception => exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                }
            };
            builder.UseExceptionHandler(options);
            return builder;
        }
    }
}
