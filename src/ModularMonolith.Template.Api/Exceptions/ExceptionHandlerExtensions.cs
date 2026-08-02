namespace ModularMonolith.Template.Api.Exceptions;

public static class ExceptionHandlerExtensions
{
    extension(IApplicationBuilder app)
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
            app.UseExceptionHandler(options);
            return app;
        }
    }
}