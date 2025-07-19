using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public class FunctionInvocationLoggingFilter(IAnsiConsole console, ILogger<FunctionInvocationLoggingFilter> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Before function invocation
        console.MarkupLineInterpolated($"[dim blue]🔧 Invoking tool: {context.Function.Name}[/]");
        logger.LogDebug("Function invoking: {FunctionName} with arguments: {Arguments}", 
            context.Function.Name, 
            string.Join(", ", context.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        // Invoke the function
        await next(context);

        // After function invocation
        console.MarkupLineInterpolated($"[dim green]✅ Tool completed: {context.Function.Name}[/]");
        logger.LogDebug("Function invoked: {FunctionName}, result: {Result}", 
            context.Function.Name, 
            context.Result?.GetValue<object>()?.ToString() ?? "null");
    }
}