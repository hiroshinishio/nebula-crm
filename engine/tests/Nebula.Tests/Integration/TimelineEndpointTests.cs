using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nebula.Application.DTOs;

namespace Nebula.Tests.Integration;

/// <summary>
/// Integration tests for timeline pagination contract (F0002-S0007, G4).
/// </summary>
public class TimelineEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<BrokerDto> CreateBrokerAsync(string license)
    {
        var response = await _client.PostAsJsonAsync("/brokers",
            new BrokerCreateDto("Timeline Test Broker", license, "CA", null, null));
        return (await response.Content.ReadFromJsonAsync<BrokerDto>())!;
    }

    [Fact]
    public async Task GetTimeline_ReturnsPaginatedEnvelope()
    {
        var broker = await CreateBrokerAsync("TL-PAG-001");

        var response = await _client.GetAsync(
            $"/timeline/events?entityType=Broker&entityId={broker.Id}&page=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonPaginatedTimelineList>();
        json.Should().NotBeNull();
        json!.Data.Should().NotBeNull();
        json.Page.Should().Be(1);
        json.PageSize.Should().Be(50);
        json.TotalCount.Should().BeGreaterThanOrEqualTo(1); // BrokerCreated event exists
        json.TotalPages.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetTimeline_DefaultPageSize_Is50()
    {
        var broker = await CreateBrokerAsync("TL-PAG-002");

        var response = await _client.GetAsync(
            $"/timeline/events?entityType=Broker&entityId={broker.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonPaginatedTimelineList>();
        json!.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetTimeline_Page2_ReturnsEmptyDataWhenNotEnoughEvents()
    {
        var broker = await CreateBrokerAsync("TL-PAG-003");

        var response = await _client.GetAsync(
            $"/timeline/events?entityType=Broker&entityId={broker.Id}&page=2&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonPaginatedTimelineList>();
        json!.Data.Should().BeEmpty();
        json.Page.Should().Be(2);
    }

    private record JsonPaginatedTimelineList(
        IReadOnlyList<TimelineEventDto> Data, int Page, int PageSize, int TotalCount, int TotalPages);
}
