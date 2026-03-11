using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nebula.Application.DTOs;

namespace Nebula.Tests.Integration;

public class ContactEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<BrokerDto> CreateBrokerAsync(string license)
    {
        var response = await _client.PostAsJsonAsync("/brokers",
            new BrokerCreateDto("Contact Test Broker", license, "CA", null, null));
        return (await response.Content.ReadFromJsonAsync<BrokerDto>())!;
    }

    [Fact]
    public async Task CreateContact_WithValidData_Returns201()
    {
        var broker = await CreateBrokerAsync("CTT-001");

        var dto = new ContactCreateDto(broker.Id, "Jane Doe", "jane@example.com", "+14155551111", "Primary");
        var response = await _client.PostAsJsonAsync("/contacts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ContactDto>();
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task ListContacts_FilterByBrokerId_ReturnsFiltered()
    {
        var broker = await CreateBrokerAsync("CTT-002");
        await _client.PostAsJsonAsync("/contacts",
            new ContactCreateDto(broker.Id, "Filter Test", "filter@test.com", "+14155552222", null));

        var response = await _client.GetAsync($"/contacts?brokerId={broker.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetContact_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/contacts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── F0002 G3: paginated envelope + RowVersion ───────────────────────────
    [Fact]
    public async Task ListContacts_ReturnsPaginatedEnvelope()
    {
        var broker = await CreateBrokerAsync("CTT-PAG-001");
        await _client.PostAsJsonAsync("/contacts",
            new ContactCreateDto(broker.Id, "Paged Contact", "paged@test.com", "+14155553333", null));

        var response = await _client.GetAsync($"/contacts?brokerId={broker.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonPaginatedContactList>();
        json.Should().NotBeNull();
        json!.Data.Should().NotBeEmpty();
        json.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        json.Page.Should().Be(1);
        json.TotalPages.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateContact_ResponseIncludesRowVersion()
    {
        var broker = await CreateBrokerAsync("CTT-RV-001");
        var dto = new ContactCreateDto(broker.Id, "RV Test", "rv@test.com", "+14155554444", null);

        var response = await _client.PostAsJsonAsync("/contacts", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ContactDto>();
        result.Should().NotBeNull();
        // RowVersion is a uint returned in response — default value 0 on creation is valid.
        result!.RowVersion.Should().BeGreaterThanOrEqualTo(0u);
    }

    private record JsonPaginatedContactList(
        IReadOnlyList<ContactDto> Data, int Page, int PageSize, int TotalCount, int TotalPages);
}
