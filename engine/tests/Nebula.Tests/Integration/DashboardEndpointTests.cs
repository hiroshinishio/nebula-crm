using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nebula.Application.DTOs;

namespace Nebula.Tests.Integration;

public class DashboardEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetKpis_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/kpis");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var kpis = await response.Content.ReadFromJsonAsync<DashboardKpisDto>();
        kpis.Should().NotBeNull();
        kpis!.ActiveBrokers.Should().BeGreaterThanOrEqualTo(0);
        kpis.OpenSubmissions.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetOpportunities_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var opportunities = await response.Content.ReadFromJsonAsync<DashboardOpportunitiesDto>();
        opportunities.Should().NotBeNull();
        opportunities!.Submissions.Should().NotBeNull();
        opportunities.Renewals.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOpportunityFlow_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/flow?entityType=submission&periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var flow = await response.Content.ReadFromJsonAsync<OpportunityFlowDto>();
        flow.Should().NotBeNull();
        flow!.EntityType.Should().Be("submission");
        flow.Nodes.Should().NotBeNull();
        flow.Links.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNudges_Returns200()
    {
        var response = await _client.GetAsync("/dashboard/nudges");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyTasks_Returns200()
    {
        var response = await _client.GetAsync("/my/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTimelineEvents_Returns200()
    {
        var response = await _client.GetAsync("/timeline/events?entityType=Broker");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
