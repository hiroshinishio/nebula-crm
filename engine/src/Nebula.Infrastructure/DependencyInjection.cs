using Microsoft.Extensions.DependencyInjection;
using Nebula.Application.Interfaces;
using Nebula.Infrastructure.Authorization;
using Nebula.Infrastructure.Persistence;
using Nebula.Infrastructure.Repositories;

namespace Nebula.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IRenewalRepository, RenewalRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITimelineRepository, TimelineRepository>();
        services.AddScoped<IWorkflowTransitionRepository, WorkflowTransitionRepository>();
        services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IAuthorizationService, PolicyAuthorizationService>();
        services.AddMemoryCache();

        return services;
    }
}
