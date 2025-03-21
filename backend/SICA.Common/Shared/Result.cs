namespace SICA.Common.Shared;

public record Result
{
    public required bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public required string? ErrorMessage { get; init; }
    public required Exception? Exception { get; init; }

    public static Result Success()
    {
        return new Result
        {
            IsSuccess = true,
            ErrorMessage = null,
            Exception = null
        };
    }

    public static Result Failure(
        string? message,
        Exception? exception = null)
    {
        return new Result
        {
            IsSuccess = false,
            ErrorMessage = message,
            Exception = exception
        };
    }

    public static Result<TValue> Success<TValue>(
        TValue value)
    {
        return new Result<TValue>
        {
            IsSuccess = true,
            Value = value,
            ErrorMessage = null,
            Exception = null
        };
    }

    public static Result<TValue> Failure<TValue>(
        string? message,
        Exception? exception = null)
    {
        return new Result<TValue>
        {
            IsSuccess = false,
            Value = default,
            ErrorMessage = message,
            Exception = exception
        };
    }

    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess()
            : onFailure(ErrorMessage!);
    }

    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string, Exception, TResult> onFailure)
    {
        return IsSuccess 
            ? onSuccess()
            : onFailure(ErrorMessage!, Exception!);
    }
}

public record Result<TValue>
{
    public required bool IsSuccess { get; init; }
    public required TValue? Value { get; init; }
    public required string? ErrorMessage { get; init; }
    public required Exception? Exception { get; init; }
    public bool IsFailure => !IsSuccess;

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(ErrorMessage!);
    }

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<string, Exception, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(ErrorMessage!, Exception!);
    }
}

public static class ResultExtensions
{
    public static async Task<TResult> MatchAsync<TResult>(
        this Task<Result> resultTask,
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAsync<TResult>(
        this Task<Result> resultTask,
        Func<TResult> onSuccess,
        Func<string, Exception, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAsync<TResult, TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAsync<TResult, TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> onSuccess,
        Func<string, Exception, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static TResult MatchAll<TResult>(
        this IEnumerable<Result> results,
        Func<TResult> onSuccess,
        Func<IEnumerable<string>, TResult> onFailure)
    {
        var failed = results.Where(r => r.IsFailure).ToList();
        return failed.Any()
            ? onFailure(failed.Select(f => f.ErrorMessage!))
            : onSuccess();
    }

    public static TResult MatchAll<TResult>(
        this IEnumerable<Result> results,
        Func<TResult> onSuccess,
        Func<IEnumerable<Tuple<string, Exception>>, TResult> onFailure)
    {
        var failed = results.Where(r => r.IsFailure).ToList();
        return failed.Any() ? onFailure(failed.Select(f =>
            Tuple.Create(f.ErrorMessage!, f.Exception!))) : onSuccess();
    }

    public static TResult MatchAll<TResult, TValue>(
        this IEnumerable<Result<TValue>> results,
        Func<IEnumerable<TValue>, TResult> onSuccess,
        Func<IEnumerable<string>, TResult> onFailure)
    {
        var resultsList = results.ToList();
        var failed = resultsList.Where(r => r.IsFailure)
            .ToList();
        if (failed.Any())
        {
            return onFailure(failed.Select(f => f.ErrorMessage!));
        }

        var values = resultsList.Select(r => r.Value!);
        return onSuccess(values);
    }

    public static TResult MatchAll<TResult, TValue>(
        this IEnumerable<Result<TValue>> results,
        Func<IEnumerable<TValue>, TResult> onSuccess,
        Func<IEnumerable<Tuple<string, Exception>>, TResult> onFailure)
    {
        var resultsList = results.ToList();
        var failed = resultsList.Where(r => r.IsFailure)
            .ToList();
        if (failed.Any())
        {
            return onFailure(failed.Select(f =>
                Tuple.Create(f.ErrorMessage!, f.Exception!)));
        }

        var values = resultsList.Select(r => r.Value!);
        return onSuccess(values);
    }

    public static async Task<TResult> MatchAllAsync<TResult>(
        this IEnumerable<Task<Result>> resultTasks,
        Func<TResult> onSuccess,
        Func<IEnumerable<string>, TResult> onFailure)
    {
        var results = await Task.WhenAll(resultTasks);
        return MatchAll(results, onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAllAsync<TResult>(
        this IEnumerable<Task<Result>> resultTasks,
        Func<TResult> onSuccess,
        Func<IEnumerable<Tuple<string, Exception>>, TResult> onFailure)
    {
        var results = await Task.WhenAll(resultTasks);
        return MatchAll(results, onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAllAsync<TResult, TValue>(
        this IEnumerable<Task<Result<TValue>>> resultTasks,
        Func<IEnumerable<TValue>, TResult> onSuccess,
        Func<IEnumerable<string>, TResult> onFailure)
    {
        var results = await Task.WhenAll(resultTasks);
        return MatchAll(results, onSuccess, onFailure);
    }

    public static async Task<TResult> MatchAllAsync<TResult, TValue>(
        this IEnumerable<Task<Result<TValue>>> resultTasks,
        Func<IEnumerable<TValue>, TResult> onSuccess,
        Func<IEnumerable<Tuple<string, Exception>>, TResult> onFailure)
    {
        var results = await Task.WhenAll(resultTasks);
        return MatchAll(results, onSuccess, onFailure);
    }
}