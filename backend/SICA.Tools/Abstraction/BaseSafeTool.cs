using Microsoft.Extensions.Logging;
using SICA.Common.Shared;

namespace SICA.Tools.Abstraction;

internal abstract class BaseSafeTool<TTool>
    where TTool : class
{
    protected async Task<Result<TResult>> SafeExecuteAsync<TResult>(Func<Task<Result<TResult>>> func, ILogger<TTool> logger)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            return Result<TResult>.Failure($"An error occurred: {ex.Message}");
        }
    }

    protected async Task<Result> SafeExecuteAsync(Func<Task<Result>> func, ILogger<TTool> logger)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            return Result.Failure($"An error occurred: {ex.Message}");
        }
    }
}