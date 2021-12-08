using Microsoft.Extensions.DependencyInjection;
using TasksTracker.Contracts.Interfaces;
using TasksTracker.Sqlite.Repositories;

namespace TasksTracker.Sqlite;

public static class ServiceRegistration
{
    public static IServiceCollection AddServiceRegistration(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        return services;
    }
}