using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

namespace TasksTracker.Cli.Infrastructure;

public sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

#pragma warning disable CS8767
    public object Resolve(Type type)
#pragma warning restore CS8767
    {
        return _provider.GetRequiredService(type);
    }
}