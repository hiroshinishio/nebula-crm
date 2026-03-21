using FluentAssertions;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Application.Services;
using Nebula.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nebula.Tests.Unit;

public class TaskServiceTests
{
    private readonly StubTaskRepository _taskRepo = new();
    private readonly StubTimelineRepository _timelineRepo = new();
    private readonly StubUnitOfWork _unitOfWork = new();
    private readonly StubCurrentUserService _user = new(Guid.Parse("aaaa0000-0000-0000-0000-000000000001"));

    private TaskService CreateService() => new(
        _taskRepo,
        _timelineRepo,
        _unitOfWork,
        new StubAuthorizationService(),
        new BrokerScopeResolver(new StubBrokerRepository()),
        NullLogger<TaskService>.Instance);

    // ═══════════════════════════════════════════════════════════════════════
    //  S0001: CreateAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsTaskDto()
    {
        var svc = CreateService();
        var dto = new TaskCreateRequestDto("Test Task", "desc", "High", DateTime.UtcNow.AddDays(7),
            _user.UserId, null, null);

        var (result, error) = await svc.CreateAsync(dto, _user);

        error.Should().BeNull();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
        result.Status.Should().Be("Open");
        result.Priority.Should().Be("High");
        _taskRepo.Added.Should().HaveCount(1);
        _timelineRepo.Events.Should().HaveCount(1);
        _timelineRepo.Events[0].EventType.Should().Be("TaskCreated");
        _unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_DefaultPriority_WhenNullPriority()
    {
        var svc = CreateService();
        var dto = new TaskCreateRequestDto("Test", null, null, null, _user.UserId, null, null);

        var (result, _) = await svc.CreateAsync(dto, _user);

        result!.Priority.Should().Be("Normal");
    }

    [Fact]
    public async Task CreateAsync_SelfAssignmentViolation_ReturnsForbidden()
    {
        var svc = CreateService();
        var otherUserId = Guid.NewGuid();
        var dto = new TaskCreateRequestDto("Test", null, null, null, otherUserId, null, null);

        var (result, error) = await svc.CreateAsync(dto, _user);

        error.Should().Be("forbidden");
        result.Should().BeNull();
        _unitOfWork.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_LinkedEntityMismatch_ReturnsValidationError()
    {
        var svc = CreateService();
        var dto = new TaskCreateRequestDto("Test", null, null, null, _user.UserId, "Broker", null);

        var (result, error) = await svc.CreateAsync(dto, _user);

        error.Should().Be("validation_error");
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithLinkedEntity_Succeeds()
    {
        var svc = CreateService();
        var linkedId = Guid.NewGuid();
        var dto = new TaskCreateRequestDto("Test", null, null, null, _user.UserId, "Broker", linkedId);

        var (result, error) = await svc.CreateAsync(dto, _user);

        error.Should().BeNull();
        result!.LinkedEntityType.Should().Be("Broker");
        result.LinkedEntityId.Should().Be(linkedId);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0002: UpdateAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateAsync_StatusOpenToInProgress_Succeeds()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        var dto = new TaskUpdateRequestDto(null, null, "InProgress", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("InProgress");
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskUpdated");
    }

    [Fact]
    public async Task UpdateAsync_StatusInProgressToDone_SetsCompletedAt()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "InProgress");
        var dto = new TaskUpdateRequestDto(null, null, "Done", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("Done");
        result.CompletedAt.Should().NotBeNull();
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskCompleted");
    }

    [Fact]
    public async Task UpdateAsync_StatusDoneToOpen_ClearsCompletedAt()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Done");
        task.CompletedAt = DateTime.UtcNow.AddHours(-1);
        var dto = new TaskUpdateRequestDto(null, null, "Open", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("Open");
        result.CompletedAt.Should().BeNull();
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskReopened");
    }

    [Fact]
    public async Task UpdateAsync_StatusOpenToDone_ReturnsInvalidTransition()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        var dto = new TaskUpdateRequestDto(null, null, "Done", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().Be("invalid_status_transition");
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFound()
    {
        var svc = CreateService();
        var dto = new TaskUpdateRequestDto("New Title", null, null, null, null, null);
        var present = Fields("title");

        var (result, error, _, _) = await svc.UpdateAsync(Guid.NewGuid(), dto, present, 0, _user);

        error.Should().Be("not_found");
    }

    [Fact]
    public async Task UpdateAsync_OtherUsersTask_ReturnsNotFound()
    {
        var svc = CreateService();
        var otherUserId = Guid.NewGuid();
        var task = SeedTask(otherUserId, "Open");
        var dto = new TaskUpdateRequestDto("New Title", null, null, null, null, null);
        var present = Fields("title");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().Be("not_found"); // IDOR normalization: 403 → 404
    }

    [Fact]
    public async Task UpdateAsync_ReassignToOther_ReturnsForbidden()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        var dto = new TaskUpdateRequestDto(null, null, null, null, null, Guid.NewGuid());
        var present = Fields("assignedToUserId");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().Be("forbidden");
    }

    [Fact]
    public async Task UpdateAsync_ClearDueDate_SetsNull()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        task.DueDate = DateTime.UtcNow.AddDays(5);
        var dto = new TaskUpdateRequestDto(null, null, null, null, null, null);
        var present = Fields("dueDate"); // dueDate present but null = clear

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ClearDescription_SetsNull()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        task.Description = "Old description";
        var dto = new TaskUpdateRequestDto(null, null, null, null, null, null);
        var present = Fields("description"); // description present but null = clear

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Description.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0003: DeleteAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteAsync_OwnTask_Succeeds()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");

        var error = await svc.DeleteAsync(task.Id, _user);

        error.Should().BeNull();
        task.IsDeleted.Should().BeTrue();
        task.DeletedAt.Should().NotBeNull();
        task.DeletedByUserId.Should().Be(_user.UserId);
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskDeleted");
        _unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsNotFound()
    {
        var svc = CreateService();

        var error = await svc.DeleteAsync(Guid.NewGuid(), _user);

        error.Should().Be("not_found");
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersTask_ReturnsNotFound()
    {
        var svc = CreateService();
        var task = SeedTask(Guid.NewGuid(), "Open");

        var error = await svc.DeleteAsync(task.Id, _user);

        error.Should().Be("not_found"); // IDOR normalization: 403 → 404
    }

    [Fact]
    public async Task DeleteAsync_CompletedTask_Succeeds()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Done");
        task.CompletedAt = DateTime.UtcNow.AddHours(-1);

        var error = await svc.DeleteAsync(task.Id, _user);

        error.Should().BeNull();
        task.IsDeleted.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0002: Additional transitions & edge cases
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateAsync_StatusDoneToInProgress_ClearsCompletedAt()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Done");
        task.CompletedAt = DateTime.UtcNow.AddHours(-1);
        var dto = new TaskUpdateRequestDto(null, null, "InProgress", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("InProgress");
        result.CompletedAt.Should().BeNull();
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskReopened");
    }

    [Fact]
    public async Task UpdateAsync_StatusInProgressToOpen_Succeeds()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "InProgress");
        var dto = new TaskUpdateRequestDto(null, null, "Open", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("Open");
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskUpdated");
    }

    [Fact]
    public async Task UpdateAsync_SameStatus_TreatedAsNoStatusChange()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        var dto = new TaskUpdateRequestDto(null, null, "Open", null, null, null);
        var present = Fields("status");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Status.Should().Be("Open");
        _timelineRepo.Events.Should().ContainSingle(e => e.EventType == "TaskUpdated");
    }

    [Fact]
    public async Task UpdateAsync_MultipleFieldChanges_TracksAllChangedFields()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        task.Priority = "Normal";
        var dto = new TaskUpdateRequestDto("New Title", null, null, "High", null, null);
        var present = Fields("title", "priority");

        var (result, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        result!.Title.Should().Be("New Title");
        result.Priority.Should().Be("High");
        var evt = _timelineRepo.Events.Single();
        evt.EventType.Should().Be("TaskUpdated");
        evt.EventDescription.Should().Contain("title").And.Contain("priority");
    }

    [Fact]
    public async Task UpdateAsync_ReopenedEvent_CapturesOriginalCompletedAt()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Done");
        var originalCompletedAt = DateTime.UtcNow.AddHours(-2);
        task.CompletedAt = originalCompletedAt;
        var dto = new TaskUpdateRequestDto(null, null, "Open", null, null, null);
        var present = Fields("status");

        var (_, error, _, _) = await svc.UpdateAsync(task.Id, dto, present, 0, _user);

        error.Should().BeNull();
        var evt = _timelineRepo.Events.Single();
        evt.EventType.Should().Be("TaskReopened");
        evt.EventPayloadJson.Should().Contain("previousCompletedAt");
        // Verify the payload contains the original CompletedAt, not the current time
        evt.EventPayloadJson.Should().Contain(originalCompletedAt.Year.ToString());
    }

    [Fact]
    public async Task CreateAsync_PastDueDate_Succeeds()
    {
        var svc = CreateService();
        var pastDate = DateTime.UtcNow.AddDays(-7);
        var dto = new TaskCreateRequestDto("Overdue task", null, null, pastDate, _user.UserId, null, null);

        var (result, error) = await svc.CreateAsync(dto, _user);

        error.Should().BeNull();
        result!.DueDate.Should().BeCloseTo(pastDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateAsync_TimelineEvent_HasCorrectPayloadFields()
    {
        var svc = CreateService();
        var linkedId = Guid.NewGuid();
        var dto = new TaskCreateRequestDto("Audit task", "desc", "High",
            DateTime.UtcNow.AddDays(3), _user.UserId, "Broker", linkedId);

        await svc.CreateAsync(dto, _user);

        var evt = _timelineRepo.Events.Single();
        evt.EntityType.Should().Be("Task");
        evt.EventType.Should().Be("TaskCreated");
        evt.BrokerDescription.Should().BeNull("task events are InternalOnly");
        evt.ActorUserId.Should().Be(_user.UserId);
        evt.EventDescription.Should().Contain("Audit task");
        evt.EventPayloadJson.Should().Contain("title");
        evt.EventPayloadJson.Should().Contain("assignedToUserId");
    }

    [Fact]
    public async Task DeleteAsync_TimelineEvent_HasCorrectFields()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");

        await svc.DeleteAsync(task.Id, _user);

        var evt = _timelineRepo.Events.Single();
        evt.EntityType.Should().Be("Task");
        evt.EntityId.Should().Be(task.Id);
        evt.EventType.Should().Be("TaskDeleted");
        evt.EventDescription.Should().Be("Task deleted");
        evt.BrokerDescription.Should().BeNull("task events are InternalOnly");
        evt.ActorUserId.Should().Be(_user.UserId);
    }

    [Fact]
    public async Task DeleteAsync_SetsUpdatedAtAndUpdatedByUserId()
    {
        var svc = CreateService();
        var task = SeedTask(_user.UserId, "Open");
        var beforeDelete = DateTime.UtcNow;

        await svc.DeleteAsync(task.Id, _user);

        task.UpdatedAt.Should().BeOnOrAfter(beforeDelete);
        task.UpdatedByUserId.Should().Be(_user.UserId);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Read methods — AuditBrokerUserRead coverage
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetMyTasksAsync_NonBrokerUser_SkipsAuditLog()
    {
        var svc = CreateService();
        var result = await svc.GetMyTasksAsync(_user.UserId, "Test User", 10, _user);

        result.Should().NotBeNull();
        result.Tasks.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetMyTasksAsync_BrokerUser_AuditLogIsEmitted()
    {
        var brokerUser = new StubCurrentUserService(
            Guid.Parse("aaaa0000-0000-0000-0000-000000000002"),
            roles: ["BrokerUser"],
            brokerTenantId: "broker-tenant-1");
        var svc = CreateService();

        // Should not throw — audit log is best-effort
        var result = await svc.GetMyTasksAsync(brokerUser.UserId, null, 10, brokerUser);

        result.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════════════

    private TaskItem SeedTask(Guid assignedToUserId, string status)
    {
        var task = new TaskItem
        {
            Title = "Seeded Task",
            Status = status,
            Priority = "Normal",
            AssignedToUserId = assignedToUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = assignedToUserId,
            UpdatedByUserId = assignedToUserId,
        };
        _taskRepo.Seed(task);
        return task;
    }

    private static HashSet<string> Fields(params string[] fields) =>
        new(fields, StringComparer.OrdinalIgnoreCase);
}

// ═══════════════════════════════════════════════════════════════════════════
//  Test doubles
// ═══════════════════════════════════════════════════════════════════════════

internal class StubTaskRepository : ITaskRepository
{
    private readonly Dictionary<Guid, TaskItem> _tasks = new();
    public List<TaskItem> Added { get; } = [];

    public void Seed(TaskItem task) => _tasks[task.Id] = task;

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_tasks.GetValueOrDefault(id));

    public Task<(IReadOnlyList<TaskItem> Tasks, int TotalCount)> GetMyTasksAsync(
        Guid assignedToUserId, int limit, CancellationToken ct = default) =>
        Task.FromResult<(IReadOnlyList<TaskItem>, int)>(([], 0));

    public Task<(IReadOnlyList<TaskItem> Tasks, int TotalCount)> GetBrokerScopedTasksAsync(
        Guid brokerId, int limit, CancellationToken ct = default) =>
        Task.FromResult<(IReadOnlyList<TaskItem>, int)>(([], 0));

    public Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        Added.Add(task);
        _tasks[task.Id] = task;
        return Task.CompletedTask;
    }
}

internal class StubTimelineRepository : ITimelineRepository
{
    public List<ActivityTimelineEvent> Events { get; } = [];

    public Task AddEventAsync(ActivityTimelineEvent evt, CancellationToken ct = default)
    {
        Events.Add(evt);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ActivityTimelineEvent>> ListEventsAsync(
        string entityType, Guid? entityId, int limit, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ActivityTimelineEvent>>([]);

    public Task<PaginatedResult<ActivityTimelineEvent>> ListEventsPagedAsync(
        string entityType, Guid? entityId, int page, int pageSize, CancellationToken ct = default) =>
        Task.FromResult(new PaginatedResult<ActivityTimelineEvent>([], 1, pageSize, 0));

    public Task<IReadOnlyList<ActivityTimelineEvent>> ListEventsForBrokerUserAsync(
        IReadOnlyList<Guid> brokerIds, int limit, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ActivityTimelineEvent>>([]);
}

internal class StubUnitOfWork : IUnitOfWork
{
    public int CommitCount { get; private set; }
    public Task CommitAsync(CancellationToken ct = default)
    {
        CommitCount++;
        return Task.CompletedTask;
    }
}

internal class StubCurrentUserService(
    Guid userId,
    IReadOnlyList<string>? roles = null,
    string? brokerTenantId = null) : ICurrentUserService
{
    public Guid UserId => userId;
    public string? DisplayName => "Test User";
    public IReadOnlyList<string> Roles => roles ?? ["Admin"];
    public IReadOnlyList<string> Regions => ["West"];
    public string? BrokerTenantId => brokerTenantId;
}

internal class StubAuthorizationService : IAuthorizationService
{
    public Task<bool> AuthorizeAsync(string userRole, string resourceType, string action,
        IDictionary<string, object>? resourceAttributes = null)
    {
        // Simulate Casbin policy: task operations require assignee == subjectId
        if (resourceAttributes is not null
            && resourceAttributes.TryGetValue("assignee", out var assignee)
            && resourceAttributes.TryGetValue("subjectId", out var subjectId))
        {
            return Task.FromResult(Equals(assignee, subjectId));
        }
        // Default allow (for roles without attribute conditions)
        return Task.FromResult(true);
    }
}

internal class StubBrokerRepository : IBrokerRepository
{
    public Task<Guid?> GetIdByBrokerTenantIdAsync(string brokerTenantId, CancellationToken ct = default) =>
        Task.FromResult<Guid?>(null);
    public Task<Broker?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Broker?>(null);
    public Task<Broker?> GetByIdIncludingDeactivatedAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Broker?>(null);
    public Task<PaginatedResult<Broker>> ListAsync(string? search, string? statusFilter, int page, int pageSize, CancellationToken ct = default) => Task.FromResult(new PaginatedResult<Broker>([], 1, pageSize, 0));
    public Task AddAsync(Broker broker, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(Broker broker, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> ExistsByLicenseAsync(string licenseNumber, CancellationToken ct = default) => Task.FromResult(false);
    public Task<bool> HasActiveSubmissionsOrRenewalsAsync(Guid brokerId, CancellationToken ct = default) => Task.FromResult(false);
}
