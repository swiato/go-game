using Domain.Common;

namespace Domain.Extensions;

public static class ResultExtensions
{
    public static Result<T> OnSuccess<T>(this Result result, Func<T> func)
    {
        if (result.IsSuccess)
            return Result.Success(func());

        return Result.Failure<T>(result.Error);
    }
}
