using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Application.DTOs;

namespace Nebula.Tests.Integration;

public class TaskEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    public void Dispose()
    {
        TestAuthHandler.TestSubject = "test-user-001";
        TestAuthHandler.TestRole = "Admin";
        TestAuthHandler.TestDisplayName = "Test User";
        TestAuthHandler.ResetF0009Overrides();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0001: POST /tasks — Create Task
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateTask_WithValidData_Returns201()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Follow up with broker", "Call broker re: renewal",
            "High", DateTime.UtcNow.AddDays(7), userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Follow up with broker");
        result.Status.Should().Be("Open");
        result.Priority.Should().Be("High");
        result.AssignedToUserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateTask_DefaultPriority_ReturnsNormal()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Default priority task", null, null, null, userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Priority.Should().Be("Normal");
    }

    [Fact]
    public async Task CreateTask_SelfAssignmentViolation_Returns403()
    {
        var otherUserId = Guid.NewGuid();
        var dto = new TaskCreateRequestDto("Bad assignment", null, null, null, otherUserId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTask_MissingTitle_Returns400()
    {
        var userId = await GetCurrentUserId();
        var json = JsonSerializer.Serialize(new { assignedToUserId = userId });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/tasks", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_TitleTooLong_Returns400()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto(new string('A', 256), null, null, null, userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_InvalidPriority_Returns400()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Test", null, "Critical", null, userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_InvalidLinkedEntityType_Returns400()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Test", null, null, null, userId, "Unknown", Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_LinkedEntityTypeMissingId_Returns400()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Test", null, null, null, userId, "Broker", null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithLinkedEntity_Returns201()
    {
        var userId = await GetCurrentUserId();
        var linkedId = Guid.NewGuid();
        var dto = new TaskCreateRequestDto("Linked task", null, null, null, userId, "Submission", linkedId);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.LinkedEntityType.Should().Be("Submission");
        result.LinkedEntityId.Should().Be(linkedId);
    }

    [Fact]
    public async Task CreateTask_ExternalUser_Returns403()
    {
        TestAuthHandler.TestRole = "ExternalUser";
        TestAuthHandler.TestNebulaRoles = [];
        var dto = new TaskCreateRequestDto("Bad task", null, null, null, Guid.NewGuid(), null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0002: PUT /tasks/{taskId} — Update Task
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateTask_ChangeTitle_Returns200()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { title = "Updated Title" });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateTask_StatusOpenToInProgress_Returns200()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { status = "InProgress" });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task UpdateTask_StatusInProgressToDone_SetsCompletedAt()
    {
        var taskId = await CreateTestTask();
        // First transition to InProgress
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));
        // Then to Done
        var response = await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Done" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Status.Should().Be("Done");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTask_StatusOpenToDone_Returns409()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { status = "Done" });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateTask_StatusDoneToOpen_ClearsCompletedAt()
    {
        var taskId = await CreateTestTask();
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Done" }));

        var response = await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Open" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Status.Should().Be("Open");
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTask_EmptyPayload_Returns400()
    {
        var taskId = await CreateTestTask();

        var response = await PutJsonAsync($"/tasks/{taskId}", "{}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_NotFound_Returns404()
    {
        var json = JsonSerializer.Serialize(new { title = "X" });

        // Pass a dummy rowVersion since the task doesn't exist (can't auto-fetch)
        var response = await PutJsonAsync($"/tasks/{Guid.NewGuid()}", json, rowVersion: 1);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_MissingIfMatch_Returns428()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { title = "X" });

        // Send PUT without If-Match header
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/tasks/{taskId}", content);

        ((int)response.StatusCode).Should().Be(428);
    }

    [Fact]
    public async Task UpdateTask_InvalidStatus_Returns400()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { status = "Cancelled" });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_TitleTooLong_Returns400()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { title = new string('A', 256) });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_ExternalUser_Returns404()
    {
        var taskId = await CreateTestTask();
        TestAuthHandler.TestRole = "ExternalUser";
        TestAuthHandler.TestNebulaRoles = [];

        var response = await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { title = "X" }));

        // IDOR normalization: ExternalUser gets 404 (not 403) to prevent entity existence leakage
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0003: DELETE /tasks/{taskId} — Delete Task
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteTask_OwnTask_Returns204()
    {
        var taskId = await CreateTestTask();

        var response = await _client.DeleteAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_ThenGetReturns404()
    {
        var taskId = await CreateTestTask();
        await _client.DeleteAsync($"/tasks/{taskId}");

        var response = await _client.GetAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_AlreadyDeleted_Returns404()
    {
        var taskId = await CreateTestTask();
        await _client.DeleteAsync($"/tasks/{taskId}");

        var response = await _client.DeleteAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_CompletedTask_Returns204()
    {
        var taskId = await CreateTestTask();
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Done" }));

        var response = await _client.DeleteAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_ExternalUser_Returns404()
    {
        var taskId = await CreateTestTask();
        TestAuthHandler.TestRole = "ExternalUser";
        TestAuthHandler.TestNebulaRoles = [];

        var response = await _client.DeleteAsync($"/tasks/{taskId}");

        // IDOR normalization: ExternalUser gets 404 (not 403) to prevent entity existence leakage
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0001: Additional Create edge cases
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateTask_DescriptionTooLong_Returns400()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Test", new string('B', 2001), null, null, userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_PastDueDate_Returns201()
    {
        var userId = await GetCurrentUserId();
        var pastDate = DateTime.UtcNow.AddDays(-7);
        var dto = new TaskCreateRequestDto("Overdue task", null, null, pastDate, userId, null, null);

        var response = await _client.PostAsJsonAsync("/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.DueDate.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTask_ThenAppearsInMyTasks()
    {
        var userId = await GetCurrentUserId();
        var uniqueTitle = $"Visible-{Guid.NewGuid():N}";
        var dto = new TaskCreateRequestDto(uniqueTitle, null, null, null, userId, null, null);
        await _client.PostAsJsonAsync("/tasks", dto);

        var response = await _client.GetAsync("/my/tasks?limit=100");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain(uniqueTitle);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0002: Additional Update edge cases
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateTask_DescriptionTooLong_Returns400()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { description = new string('X', 2001) });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_InvalidPriority_Returns400()
    {
        var taskId = await CreateTestTask();
        var json = JsonSerializer.Serialize(new { priority = "Critical" });

        var response = await PutJsonAsync($"/tasks/{taskId}", json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_StatusDoneToInProgress_ClearsCompletedAt()
    {
        var taskId = await CreateTestTask();
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Done" }));

        var response = await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Status.Should().Be("InProgress");
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTask_StatusInProgressToOpen_Returns200()
    {
        var taskId = await CreateTestTask();
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));

        var response = await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "Open" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Status.Should().Be("Open");
    }

    [Fact]
    public async Task UpdateTask_ClearDueDate_SetsNull()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Due date task", null, null, DateTime.UtcNow.AddDays(5), userId, null, null);
        var createResp = await _client.PostAsJsonAsync("/tasks", dto);
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        // Send explicit null for dueDate
        var response = await PutJsonAsync($"/tasks/{created!.Id}",
            "{\"dueDate\": null}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTask_ClearDescription_SetsNull()
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto("Desc task", "Original description", null, null, userId, null, null);
        var createResp = await _client.PostAsJsonAsync("/tasks", dto);
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var response = await PutJsonAsync($"/tasks/{created!.Id}",
            "{\"description\": null}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result!.Description.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  S0003: Additional Delete edge cases
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteTask_ThenExcludedFromMyTasks()
    {
        var uniqueTitle = $"DeleteMe-{Guid.NewGuid():N}";
        var taskId = await CreateTestTask(uniqueTitle);
        await _client.DeleteAsync($"/tasks/{taskId}");

        var response = await _client.GetAsync("/my/tasks?limit=100");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain(uniqueTitle);
    }

    [Fact]
    public async Task DeleteTask_InProgressTask_Returns204()
    {
        var taskId = await CreateTestTask();
        await PutJsonAsync($"/tasks/{taskId}", JsonSerializer.Serialize(new { status = "InProgress" }));

        var response = await _client.DeleteAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Full Lifecycle
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullLifecycle_CreateUpdateCompletionReopenDelete()
    {
        // Create
        var userId = await GetCurrentUserId();
        var createDto = new TaskCreateRequestDto("Lifecycle task", "Test full lifecycle", "Normal",
            DateTime.UtcNow.AddDays(3), userId, null, null);
        var createResp = await _client.PostAsJsonAsync("/tasks", createDto);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        // Update title
        var updateResp = await PutJsonAsync($"/tasks/{task!.Id}", JsonSerializer.Serialize(new { title = "Updated lifecycle" }));
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Open → InProgress
        var progressResp = await PutJsonAsync($"/tasks/{task.Id}", JsonSerializer.Serialize(new { status = "InProgress" }));
        progressResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // InProgress → Done
        var doneResp = await PutJsonAsync($"/tasks/{task.Id}", JsonSerializer.Serialize(new { status = "Done" }));
        doneResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var done = await doneResp.Content.ReadFromJsonAsync<TaskDto>();
        done!.CompletedAt.Should().NotBeNull();

        // Reopen: Done → Open
        var reopenResp = await PutJsonAsync($"/tasks/{task.Id}", JsonSerializer.Serialize(new { status = "Open" }));
        reopenResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var reopened = await reopenResp.Content.ReadFromJsonAsync<TaskDto>();
        reopened!.CompletedAt.Should().BeNull();

        // Delete
        var deleteResp = await _client.DeleteAsync($"/tasks/{task.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify gone
        var getResp = await _client.GetAsync($"/tasks/{task.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<Guid> GetCurrentUserId()
    {
        // Create a task and read back the assignedToUserId to discover the test user's internal ID
        var tempDto = new TaskCreateRequestDto("temp", null, null, null, Guid.Empty, null, null);

        // We need to figure out the user ID. The HttpCurrentUserService maps (iss, sub) to a UserProfile.
        // For tests, we can use a trick: attempt with a known Guid and see if it works,
        // or just create via the list endpoint.
        // Simpler: read the /my/tasks endpoint which doesn't need a task ID.
        // But we need the user's internal ID for self-assignment.
        // The test auth handler uses subject "test-user-001" with issuer "http://test.local/application/o/nebula/".
        // HttpCurrentUserService does an upsert — the first call creates the UserProfile.
        // We can trigger this by calling any auth'd endpoint, then query the DB.
        // Simplest approach: call GET /my/tasks to trigger UserProfile creation, then use the factory to query.
        await _client.GetAsync("/my/tasks");

        // Now get the user ID from the DB
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Nebula.Infrastructure.Persistence.AppDbContext>();
        var profile = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            db.Set<Nebula.Domain.Entities.UserProfile>()
                .Where(u => u.IdpSubject == "test-user-001"));
        return profile!.Id;
    }

    private async Task<Guid> CreateTestTask(string title = "Test Task")
    {
        var task = await CreateTestTaskFull(title);
        return task.Id;
    }

    private async Task<TaskDto> CreateTestTaskFull(string title = "Test Task")
    {
        var userId = await GetCurrentUserId();
        var dto = new TaskCreateRequestDto(title, null, null, null, userId, null, null);
        var response = await _client.PostAsJsonAsync("/tasks", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>())!;
    }

    /// <summary>
    /// PUT with If-Match header. Pass rowVersion from the last read/create/update response.
    /// If rowVersion is 0, fetches the current RowVersion from GET /tasks/{id} first.
    /// </summary>
    private async Task<HttpResponseMessage> PutJsonAsync(string url, string jsonBody, uint rowVersion = 0)
    {
        // If caller didn't pass a rowVersion, try to fetch the current one
        if (rowVersion == 0)
        {
            // Extract task ID from URL pattern /tasks/{guid}
            var segments = url.Split('/');
            if (segments.Length >= 3 && Guid.TryParse(segments[^1], out _))
            {
                var getResp = await _client.GetAsync(url);
                if (getResp.IsSuccessStatusCode)
                {
                    var existing = await getResp.Content.ReadFromJsonAsync<TaskDto>();
                    rowVersion = existing!.RowVersion;
                }
            }
        }

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{rowVersion}\"");
        return await _client.SendAsync(request);
    }
}
