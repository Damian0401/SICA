using Microsoft.Extensions.Logging;
using SICA.Common.Shared;

namespace SICA.Tools.Abstraction;

internal abstract class BaseSafeTool<TTool>
    where TTool : class
{
    protected readonly ILogger<TTool> Logger;

    protected BaseSafeTool(ILogger<TTool> logger)
    {
        Logger = logger;
    }

    protected async Task<Result<TResult>> SafeExecuteAsync<TResult>(Func<Task<Result<TResult>>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            return Result<TResult>.Failure($"An error occurred: {ex.Message}", ex);
        }
    }

    protected async Task<Result> SafeExecuteAsync(Func<Task<Result>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            return Result.Failure($"An error occurred: {ex.Message}", ex);
        }
    }
}