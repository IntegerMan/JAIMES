using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public sealed class FunctionInvocationLoggingFilter(IAnsiConsole console, ILogger<FunctionInvocationLoggingFilter> log) : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        string argsList = string.Join(", ", context.Arguments?.Select(kvp => $"{kvp.Key}: {kvp.Value}") ?? []);
        console.MarkupLine($"{DisplayHelpers.System}🔧 Auto-invoking tool: {DisplayHelpers.ToolCall}{context.Function.PluginName}.{context.Function.Name}[/]({argsList})[/]");
        log.LogDebug("Auto function invoking: {PluginName}.{FunctionName} with arguments: {Arguments}",
            context.Function.PluginName,
            context.Function.Name, 
            argsList);

        // Invoke the function and measure the time taken
        Stopwatch stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();
        
        // Get our result (Note: only string results supported at the moment)
        FunctionResult result = context.Result;
        string output = result.ToString();
        
        // Log the results
        log.LogDebug("Auto function invocation completed: {PluginName}.{FunctionName} in {ElapsedMilliseconds} ms with result: {Result}",
            context.Function.PluginName,
            context.Function.Name,
            stopwatch.ElapsedMilliseconds,
            output);
        if (string.IsNullOrWhiteSpace(output))
        {
            console.MarkupLine($"{DisplayHelpers.Error}No output returned from function invocation[/]");
        }
        else
        {
            console.MarkupLineInterpolated($"[bold lightslategrey]{output}[/]");
        }

        console.MarkupLine($"{DisplayHelpers.System}✅ Auto-invocation completed: {DisplayHelpers.ToolCall}{context.Function.PluginName}.{context.Function.Name}[/] in {stopwatch.ElapsedMilliseconds} ms[/]");
    }
}