using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Infrastructure.Persistence;
using Nebula.Infrastructure.Repositories;

namespace Nebula.Tests.Integration;

public class DashboardPartialFailureTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> _faultFactory;
    private readonly HttpClient _client;

    public DashboardPartialFailureTests(CustomWebApplicationFactory factory)
    {
        _faultFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "FixedAdmin";
                        options.DefaultChallengeScheme = "FixedAdmin";
                    })
                    .AddScheme<AuthenticationSchemeOptions, FixedAdminAuthHandler>("FixedAdmin", _ => { });

                services.RemoveAll<IDashboardRepository>();
                services.AddScoped<IDashboardRepository>(sp =>
                {
                    var db = sp.GetRequiredService<AppDbContext>();
                    var inner = new DashboardRepository(db);
                    return new FaultInjectingDashboardRepository(inner, failKpis: true);
                });
            }));

        _client = _faultFactory.CreateClient();
    }

    public void Dispose()
    {
        _faultFactory.Dispose();
    }

    [Fact]
    public async Task GetKpis_WhenRepositoryFails_ReturnsInternalErrorProblemDetails()
    {
        var response = await _client.GetAsync("/dashboard/kpis?periodDays=90");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        problem.Should().NotBeNull();
        problem!.Should().ContainKey("code");
        problem["code"].ToString().Should().Be("internal_error");
    }

    [Fact]
    public async Task OtherDashboardEndpoints_RemainAvailable_WhenKpiEndpointFails()
    {
        var opportunitiesResponse = await _client.GetAsync("/dashboard/opportunities?periodDays=90");
        var outcomesResponse = await _client.GetAsync("/dashboard/opportunities/outcomes?periodDays=90");
        var nudgesResponse = await _client.GetAsync("/dashboard/nudges");

        opportunitiesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        outcomesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        nudgesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class FaultInjectingDashboardRepository(
        IDashboardRepository inner,
        bool failKpis) : IDashboardRepository
    {
        public Task<DashboardKpisDto> GetKpisAsync(int periodDays = 90, CancellationToken ct = default)
        {
            if (!failKpis)
                return inner.GetKpisAsync(periodDays, ct);

            return Task.FromException<DashboardKpisDto>(
                new InvalidOperationException("Simulated KPI repository failure."));
        }

        public Task<DashboardOpportunitiesDto> GetOpportunitiesAsync(int periodDays = 180, CancellationToken ct = default) =>
            inner.GetOpportunitiesAsync(periodDays, ct);

        public Task<OpportunityFlowDto> GetOpportunityFlowAsync(string entityType, int periodDays, CancellationToken ct = default) =>
            inner.GetOpportunityFlowAsync(entityType, periodDays, ct);

        public Task<OpportunityItemsDto> GetOpportunityItemsAsync(string entityType, string status, CancellationToken ct = default) =>
            inner.GetOpportunityItemsAsync(entityType, status, ct);

        public Task<OpportunityAgingDto> GetOpportunityAgingAsync(string entityType, int periodDays, CancellationToken ct = default) =>
            inner.GetOpportunityAgingAsync(entityType, periodDays, ct);

        public Task<OpportunityHierarchyDto> GetOpportunityHierarchyAsync(int periodDays, CancellationToken ct = default) =>
            inner.GetOpportunityHierarchyAsync(periodDays, ct);

        public Task<OpportunityOutcomesDto> GetOpportunityOutcomesAsync(int periodDays, CancellationToken ct = default) =>
            inner.GetOpportunityOutcomesAsync(periodDays, ct);

        public Task<OpportunityItemsDto> GetOpportunityOutcomeItemsAsync(string outcomeKey, int periodDays, CancellationToken ct = default) =>
            inner.GetOpportunityOutcomeItemsAsync(outcomeKey, periodDays, ct);

        public Task<IReadOnlyList<NudgeCardDto>> GetNudgesAsync(Guid userId, CancellationToken ct = default) =>
            inner.GetNudgesAsync(userId, ct);

        public Task<IReadOnlyList<NudgeCardDto>> GetNudgesForBrokerUserAsync(IReadOnlyList<Guid> brokerIds, CancellationToken ct = default) =>
            inner.GetNudgesForBrokerUserAsync(brokerIds, ct);
    }

    private sealed class FixedAdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new("iss", "http://test.local/application/o/nebula/"),
                new("sub", "fixed-admin-user"),
                new(ClaimTypes.NameIdentifier, "fixed-admin-user"),
                new("name", "Fixed Admin"),
                new(ClaimTypes.Name, "Fixed Admin"),
                new("role", "Admin"),
                new(ClaimTypes.Role, "Admin"),
                new("nebula_roles", "Admin"),
                new("regions", "West"),
            };

            var identity = new ClaimsIdentity(claims, "FixedAdmin");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "FixedAdmin");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
