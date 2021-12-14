using Dapper;
using Microsoft.Extensions.DependencyInjection;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Sqlite.Repositories;

namespace TasksTracker.Sqlite;

public static class ServiceRegistration
{
    public static IServiceCollection AddServiceRegistration(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<ITaskEntryRepository, TaskEntryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaskEntryLinkRepository, TaskEntryLinkRepository>();
        
        SqlMapper.AddTypeHandler<Guid>(new GuidHandler());
        
        return services;
    }
}