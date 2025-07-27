using JetBrains.Annotations;

namespace AiTableTopGameMaster.Core.Cores;

[UsedImplicitly]
public class CoreInfo
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string[] Instructions { get; init; } = [];
    public bool IncludeHistory { get; init; }
    public bool IncludePlayerInput { get; init; }
    public string[] Plugins { get; init; } = [];
}