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
    public async Task GetOpportunities_WithPeriodDays_Returns200()
    {
        var response = await _client.GetAsync("/dashboard/opportunities?periodDays=90");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var opportunities = await response.Content.ReadFromJsonAsync<DashboardOpportunitiesDto>();
        opportunities.Should().NotBeNull();
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
    public async Task GetOpportunityAging_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/aging?entityType=submission&periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var aging = await response.Content.ReadFromJsonAsync<OpportunityAgingDto>();
        aging.Should().NotBeNull();
        aging!.EntityType.Should().Be("submission");
        aging.PeriodDays.Should().Be(180);
        aging.Statuses.Should().NotBeNull();
        foreach (var status in aging.Statuses)
        {
            status.Buckets.Should().HaveCount(5);
            status.Buckets.Select(b => b.Key).Should().ContainInOrder("0-2", "3-5", "6-10", "11-20", "21+");
            status.Total.Should().Be(status.Buckets.Sum(b => b.Count));
        }
    }

    [Theory]
    [InlineData("submission")]
    [InlineData("renewal")]
    public async Task GetOpportunityAging_SupportsEntityTypes(string entityType)
    {
        var response = await _client.GetAsync($"/dashboard/opportunities/aging?entityType={entityType}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var aging = await response.Content.ReadFromJsonAsync<OpportunityAgingDto>();
        aging!.EntityType.Should().Be(entityType);
    }

    [Fact]
    public async Task GetOpportunityAging_InvalidEntityType_Returns400()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/aging?entityType=invalid");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOpportunityHierarchy_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/hierarchy?periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var hierarchy = await response.Content.ReadFromJsonAsync<OpportunityHierarchyDto>();
        hierarchy.Should().NotBeNull();
        hierarchy!.PeriodDays.Should().Be(180);
        hierarchy.Root.Should().NotBeNull();
        hierarchy.Root.Id.Should().Be("root");
        hierarchy.Root.Children.Should().NotBeNull();
        hierarchy.Root.Children.Should().HaveCount(2);
        hierarchy.Root.Children![0].Id.Should().Be("submission");
        hierarchy.Root.Children[1].Id.Should().Be("renewal");
    }

    [Fact]
    public async Task GetOpportunityHierarchy_ChildCountsRollUp()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/hierarchy");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var hierarchy = await response.Content.ReadFromJsonAsync<OpportunityHierarchyDto>();
        var root = hierarchy!.Root;

        // Root count should equal sum of entity type children
        root.Count.Should().Be(root.Children!.Sum(c => c.Count));

        // Each entity type count should equal sum of color group children
        foreach (var entityNode in root.Children!)
        {
            if (entityNode.Children is { Count: > 0 })
                entityNode.Count.Should().Be(entityNode.Children.Sum(c => c.Count));
        }
    }

    [Fact]
    public async Task GetOpportunityOutcomes_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/outcomes?periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var outcomes = await response.Content.ReadFromJsonAsync<OpportunityOutcomesDto>();
        outcomes.Should().NotBeNull();
        outcomes!.PeriodDays.Should().Be(180);
        outcomes.Outcomes.Should().NotBeNull();
        outcomes.Outcomes.Should().Contain(o => o.Key == "bound");
        outcomes.Outcomes.Should().Contain(o => o.Key == "no_quote");
        outcomes.Outcomes.Should().Contain(o => o.Key == "declined");
        outcomes.Outcomes.Should().Contain(o => o.Key == "expired");
        outcomes.Outcomes.Should().Contain(o => o.Key == "lost_competitor");
    }

    [Fact]
    public async Task GetOpportunityOutcomeItems_Returns200WithCorrectShape()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/outcomes/bound/items?periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<OpportunityItemsDto>();
        items.Should().NotBeNull();
        items!.Items.Should().NotBeNull();
        items.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetOpportunityOutcomeItems_InvalidOutcomeKey_Returns400()
    {
        var response = await _client.GetAsync("/dashboard/opportunities/outcomes/invalid/items?periodDays=180");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
