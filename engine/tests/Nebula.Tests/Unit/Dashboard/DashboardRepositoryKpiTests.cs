using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nebula.Infrastructure.Persistence;
using Nebula.Infrastructure.Repositories;
using Nebula.Domain.Entities;

namespace Nebula.Tests.Unit.Dashboard;

public class DashboardRepositoryKpiTests
{
    [Fact]
    public async Task GetKpisAsync_UsesPeriodWindowForRenewalRateAndTurnaround()
    {
        await using var db = CreateContext();
        var now = DateTime.UtcNow;
        SeedReferenceStatuses(db);

        var brokerA = new Broker { Id = Guid.NewGuid(), Status = "Active", LegalName = "A", LicenseNumber = "L1", State = "CA" };
        var brokerB = new Broker { Id = Guid.NewGuid(), Status = "Active", LegalName = "B", LicenseNumber = "L2", State = "TX" };
        db.Brokers.AddRange(
            brokerA,
            brokerB);

        var accountId = Guid.NewGuid();
        var brokerId = brokerA.Id;
        var userId = Guid.NewGuid();

        var openSubmission = new Submission
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            BrokerId = brokerId,
            AssignedToUserId = userId,
            CurrentStatus = "Received",
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-2),
        };
        var recentSubmission = new Submission
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            BrokerId = brokerId,
            AssignedToUserId = userId,
            CurrentStatus = "Bound",
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-10),
        };
        var midSubmission = new Submission
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            BrokerId = brokerId,
            AssignedToUserId = userId,
            CurrentStatus = "Bound",
            CreatedAt = now.AddDays(-100),
            UpdatedAt = now.AddDays(-60),
        };
        db.Submissions.AddRange(openSubmission, recentSubmission, midSubmission);

        db.Renewals.AddRange(
            new Renewal
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                BrokerId = brokerId,
                AssignedToUserId = userId,
                CurrentStatus = "Bound",
                RenewalDate = now.AddDays(30),
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-40),
            },
            new Renewal
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                BrokerId = brokerId,
                AssignedToUserId = userId,
                CurrentStatus = "Lost",
                RenewalDate = now.AddDays(30),
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-5),
            },
            new Renewal
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                BrokerId = brokerId,
                AssignedToUserId = userId,
                CurrentStatus = "Bound",
                RenewalDate = now.AddDays(30),
                CreatedAt = now.AddDays(-200),
                UpdatedAt = now.AddDays(-150),
            });

        db.WorkflowTransitions.AddRange(
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                WorkflowType = "Submission",
                EntityId = recentSubmission.Id,
                FromState = "InReview",
                ToState = "Bound",
                ActorUserId = userId,
                OccurredAt = now.AddDays(-10),
            },
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                WorkflowType = "Submission",
                EntityId = midSubmission.Id,
                FromState = "InReview",
                ToState = "Bound",
                ActorUserId = userId,
                OccurredAt = now.AddDays(-60),
            });

        await db.SaveChangesAsync();

        var repository = new DashboardRepository(db);
        var ninetyDayKpis = await repository.GetKpisAsync(90);
        var thirtyDayKpis = await repository.GetKpisAsync(30);

        ninetyDayKpis.ActiveBrokers.Should().Be(2);
        thirtyDayKpis.ActiveBrokers.Should().Be(2);
        ninetyDayKpis.OpenSubmissions.Should().Be(1);
        thirtyDayKpis.OpenSubmissions.Should().Be(1);

        ninetyDayKpis.RenewalRate.Should().Be(50.0);
        thirtyDayKpis.RenewalRate.Should().Be(0.0);

        ninetyDayKpis.AvgTurnaroundDays.Should().Be(25.0);
        thirtyDayKpis.AvgTurnaroundDays.Should().Be(10.0);
    }

    [Fact]
    public async Task GetKpisAsync_AppliesDefaultAndMaximumPeriodBounds()
    {
        await using var db = CreateContext();
        var now = DateTime.UtcNow;
        SeedReferenceStatuses(db);

        var broker = new Broker { Id = Guid.NewGuid(), Status = "Active", LegalName = "A", LicenseNumber = "L1", State = "CA" };
        db.Brokers.Add(broker);

        var accountId = Guid.NewGuid();
        var brokerId = broker.Id;
        var userId = Guid.NewGuid();

        db.Submissions.Add(new Submission
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            BrokerId = brokerId,
            AssignedToUserId = userId,
            CurrentStatus = "Received",
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-1),
        });

        db.Renewals.AddRange(
            new Renewal
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                BrokerId = brokerId,
                AssignedToUserId = userId,
                CurrentStatus = "Bound",
                RenewalDate = now.AddDays(30),
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-20),
            },
            new Renewal
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                BrokerId = brokerId,
                AssignedToUserId = userId,
                CurrentStatus = "Lost",
                RenewalDate = now.AddDays(30),
                CreatedAt = now.AddDays(-500),
                UpdatedAt = now.AddDays(-400),
            });

        await db.SaveChangesAsync();

        var repository = new DashboardRepository(db);
        var defaultWindowKpis = await repository.GetKpisAsync(0);
        var maxWindowKpis = await repository.GetKpisAsync(1000);

        defaultWindowKpis.RenewalRate.Should().Be(100.0);
        maxWindowKpis.RenewalRate.Should().Be(50.0);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"kpi-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedReferenceStatuses(AppDbContext db)
    {
        db.ReferenceSubmissionStatuses.AddRange(
            new ReferenceSubmissionStatus { Code = "Received", DisplayName = "Received", Description = "Received", IsTerminal = false, DisplayOrder = 1, ColorGroup = "intake" },
            new ReferenceSubmissionStatus { Code = "Bound", DisplayName = "Bound", Description = "Bound", IsTerminal = true, DisplayOrder = 2, ColorGroup = "decision" });

        db.ReferenceRenewalStatuses.AddRange(
            new ReferenceRenewalStatus { Code = "Created", DisplayName = "Created", Description = "Created", IsTerminal = false, DisplayOrder = 1, ColorGroup = "intake" },
            new ReferenceRenewalStatus { Code = "Bound", DisplayName = "Bound", Description = "Bound", IsTerminal = true, DisplayOrder = 2, ColorGroup = "decision" },
            new ReferenceRenewalStatus { Code = "Lost", DisplayName = "Lost", Description = "Lost", IsTerminal = true, DisplayOrder = 3, ColorGroup = "decision" });
    }
}
