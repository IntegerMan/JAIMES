using AiTableTopGameMaster.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Cores;

public class CoreFactory(ILoggerFactory loggerFactory, IModelFactory factory, IKernelBuilder builder)
{
    public AiCore CreateCore(CoreInfo coreInfo)
    {
        ArgumentNullException.ThrowIfNull(coreInfo);
        
        factory.ConfigureKernel(builder, coreInfo);
        Kernel kernel = builder.Build();

        return new AiCore(kernel, coreInfo, loggerFactory);
    }
}