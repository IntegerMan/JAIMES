using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp;

public sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) => builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) => builder.AddSingleton(service, _ => factory());

    private sealed class TypeResolver(ServiceProvider provider) : ITypeResolver, IDisposable
    {
        public object? Resolve(Type? type) => provider.GetService(type ?? throw new ArgumentNullException(nameof(type)));

        public void Dispose() => provider.Dispose();
    }
}