using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public sealed class FunctionInvocationLoggingFilter(IAnsiConsole console, ILogger<FunctionInvocationLoggingFilter> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Before function invocation
        var stopwatch = Stopwatch.StartNew();
        console.MarkupLineInterpolated($"[dim blue]🔧 Invoking tool: {context.Function.Name}[/]");
        logger.LogDebug("Function invoking: {FunctionName} with arguments: {Arguments}", 
            context.Function.Name, 
            string.Join(", ", context.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        try
        {
            // Invoke the function
            await next(context);
            
            stopwatch.Stop();
            
            // After successful function invocation
            console.MarkupLineInterpolated($"[dim green]✅ Tool completed: {context.Function.Name} ({stopwatch.ElapsedMilliseconds}ms)[/]");
            logger.LogDebug("Function invoked: {FunctionName}, result: {Result}, duration: {Duration}ms", 
                context.Function.Name, 
                context.Result?.GetValue<object>()?.ToString() ?? "null",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // After failed function invocation
            console.MarkupLineInterpolated($"[dim red]❌ Tool failed: {context.Function.Name} ({stopwatch.ElapsedMilliseconds}ms)[/]");
            logger.LogError(ex, "Function invocation failed: {FunctionName}, duration: {Duration}ms", 
                context.Function.Name,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}