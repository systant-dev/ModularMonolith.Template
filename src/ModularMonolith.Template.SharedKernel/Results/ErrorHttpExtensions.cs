using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ModularMonolith.Template.SharedKernel.Results;

public static class ErrorHttpExtensions
{
    #region Public Method Declarations

    extension(Error error)
    {
        public ProblemHttpResult ToProblem()
        {
            ArgumentNullException.ThrowIfNull(error);

            int statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorised => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            return TypedResults.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
        }
    }

    #endregion
}
