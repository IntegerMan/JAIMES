using AiTableTopGameMaster.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Cores;

public class CoreFactory(IServiceProvider services)
{
    public AiCore CreateCore(CoreInfo coreInfo)
    {
        ArgumentNullException.ThrowIfNull(coreInfo);

        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
        ModelFactory factory = services.GetRequiredService<ModelFactory>();
        
        IKernelBuilder builder = services.GetRequiredService<IKernelBuilder>();
        factory.AddChatCompletion(builder, coreInfo.ModelId);
        
        Kernel kernel = builder.Build();

        return new AiCore(kernel, coreInfo, loggerFactory);
    }
}