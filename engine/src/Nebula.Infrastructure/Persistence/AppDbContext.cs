using Microsoft.EntityFrameworkCore;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;

namespace Nebula.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public Task CommitAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<MGA> MGAs => Set<MGA>();
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<BrokerRegion> BrokerRegions => Set<BrokerRegion>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Renewal> Renewals => Set<Renewal>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ActivityTimelineEvent> ActivityTimelineEvents => Set<ActivityTimelineEvent>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<ReferenceTaskStatus> ReferenceTaskStatuses => Set<ReferenceTaskStatus>();
    public DbSet<ReferenceSubmissionStatus> ReferenceSubmissionStatuses => Set<ReferenceSubmissionStatus>();
    public DbSet<ReferenceRenewalStatus> ReferenceRenewalStatuses => Set<ReferenceRenewalStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
