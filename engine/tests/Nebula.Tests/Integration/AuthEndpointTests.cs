using System.Net;
using FluentAssertions;
using Nebula.Api.Endpoints;

namespace Nebula.Tests.Integration;

/// <summary>
/// Integration tests for POST /api/auth/logout.
///
/// These tests run against the full ASP.NET Core pipeline via
/// <see cref="CustomWebApplicationFactory"/> (Testcontainers PostgreSQL + test auth scheme).
/// The authentik revocation call is skipped in Development mode because
/// <c>Authentication:Authority</c> is not set, so no outbound HTTP is made.
/// </summary>
public class AuthEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    // The factory uses UseEnvironment("Development"), which means the JWT validation
    // is replaced by TestAuthHandler AND Authentication:Authority is absent — so
    // AuthEndpoints skips revocation silently. The integration test can therefore
    // exercise the full route + cookie-clear logic without mocking an IdP.
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Logout_WithoutCookie_Returns204AndClearsCookie()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", content: null);

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            because: "logout must return 204 No Content even when no cookie is present");

        // Assert — no body
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty(because: "204 responses must have no body");

        // Assert — Set-Cookie clears the refresh_token
        AssertRefreshTokenCookieCleared(response);
    }

    [Fact]
    public async Task Logout_WithRefreshTokenCookie_Returns204AndClearsCookie()
    {
        // Arrange — attach a refresh_token cookie to simulate a session
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Add("Cookie", $"{AuthEndpoints.RefreshTokenCookieName}=some-opaque-refresh-token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();

        AssertRefreshTokenCookieCleared(response);
    }

    [Fact]
    public async Task Logout_IsAnonymous_DoesNotRequireAuthorizationHeader()
    {
        // Arrange — deliberately do NOT send any auth token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        // (no Authorization header)

        // Act
        var response = await _client.SendAsync(request);

        // Assert — must not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            because: "§2.1 requires the endpoint to accept unauthenticated requests");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------

    private static void AssertRefreshTokenCookieCleared(HttpResponseMessage response)
    {
        response.Headers.TryGetValues("Set-Cookie", out var setCookieValues).Should().BeTrue(
            because: "logout must always emit a Set-Cookie header to clear the refresh_token");

        var setCookie = string.Join("; ", setCookieValues!);

        setCookie.Should().Contain(AuthEndpoints.RefreshTokenCookieName,
            because: "the cleared cookie must target refresh_token");
        setCookie.Should().Contain("max-age=0",
            because: "Max-Age=0 instructs the browser to immediately delete the cookie");
        setCookie.Should().ContainEquivalentOf("httponly");
        setCookie.Should().ContainEquivalentOf("secure");
        setCookie.Should().ContainEquivalentOf("samesite=strict");
        setCookie.Should().Contain("path=/");
    }
}
