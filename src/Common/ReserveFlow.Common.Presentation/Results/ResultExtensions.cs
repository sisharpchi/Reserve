using Microsoft.AspNetCore.Http;
using ReserveFlow.Common.Domain;

namespace ReserveFlow.Common.Presentation.Results;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
    }
}
