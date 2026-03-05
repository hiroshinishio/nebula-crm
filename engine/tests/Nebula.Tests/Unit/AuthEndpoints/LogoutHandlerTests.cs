using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Nebula.Api.Endpoints;

namespace Nebula.Tests.Unit.AuthEndpoints;

/// <summary>
/// Unit tests for the POST /api/auth/logout handler.
///
/// The handler is tested by calling <see cref="AuthEndpoints.LogoutAsync"/> directly
/// with a <see cref="DefaultHttpContext"/> and a stub <see cref="IHttpClientFactory"/>,
/// so there is no network dependency and the tests run fully in-process.
/// </summary>
public class LogoutHandlerTests
{
    // ------------------------------------------------------------------
    // Happy path: token present, revocation succeeds, cookie cleared, 204
    // ------------------------------------------------------------------

    [Fact]
    public async Task Logout_WithRefreshTokenCookie_CallsRevocationEndpointAndReturns204()
    {
        // Arrange
        var (httpContext, messageHandler) = BuildContext(
            refreshTokenCookieValue: "valid-refresh-token-abc",
            revocationStatusCode: HttpStatusCode.OK,
            authority: "https://idp.example.com/application/o/nebula/");

        var config = BuildConfig(authority: "https://idp.example.com/application/o/nebula/");
        var factory = new StubHttpClientFactory(messageHandler);

        // Act
        var result = await Nebula.Api.Endpoints.AuthEndpoints.LogoutAsync(httpContext, config, factory, NullLogger<Program>.Instance, CancellationToken.None);

        // Assert — handler returns 204
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();

        // Assert — revocation was called exactly once with correct token
        messageHandler.Requests.Should().HaveCount(1);
        var sentRequest = messageHandler.Requests.Single();
        sentRequest.RequestUri!.ToString().Should().EndWith("/application/o/nebula/revoke/");

        var body = await sentRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("token=valid-refresh-token-abc");
        body.Should().Contain("token_type_hint=refresh_token");

        // Assert — Set-Cookie clears the refresh_token cookie
        AssertClearCookieAppended(httpContext);
    }

    // ------------------------------------------------------------------
    // Degraded path: revocation fails (IdP unreachable) — still 204
    // ------------------------------------------------------------------

    [Fact]
    public async Task Logout_RevocationEndpointUnreachable_StillReturns204AndClearsCookie()
    {
        // Arrange
        var (httpContext, messageHandler) = BuildContext(
            refreshTokenCookieValue: "token-when-idp-is-down",
            throwOnSend: new HttpRequestException("Connection refused"),
            authority: "https://idp.example.com/application/o/nebula/");

        var config = BuildConfig(authority: "https://idp.example.com/application/o/nebula/");
        var factory = new StubHttpClientFactory(messageHandler);

        // Act — must NOT throw
        var result = await Nebula.Api.Endpoints.AuthEndpoints.LogoutAsync(httpContext, config, factory, NullLogger<Program>.Instance, CancellationToken.None);

        // Assert — 204 regardless of revocation failure
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();

        // Assert — cookie still cleared
        AssertClearCookieAppended(httpContext);
    }

    [Fact]
    public async Task Logout_RevocationEndpointReturnsNonSuccess_StillReturns204AndClearsCookie()
    {
        // Arrange
        var (httpContext, messageHandler) = BuildContext(
            refreshTokenCookieValue: "some-token",
            revocationStatusCode: HttpStatusCode.BadGateway,
            authority: "https://idp.example.com/application/o/nebula/");

        var config = BuildConfig(authority: "https://idp.example.com/application/o/nebula/");
        var factory = new StubHttpClientFactory(messageHandler);

        // Act
        var result = await Nebula.Api.Endpoints.AuthEndpoints.LogoutAsync(httpContext, config, factory, NullLogger<Program>.Instance, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
        AssertClearCookieAppended(httpContext);
    }

    // ------------------------------------------------------------------
    // No cookie present: graceful no-op — still 204 + clear cookie header
    // ------------------------------------------------------------------

    [Fact]
    public async Task Logout_NoCookiePresent_SkipsRevocationAndReturns204()
    {
        // Arrange — no refresh_token cookie set
        var (httpContext, messageHandler) = BuildContext(
            refreshTokenCookieValue: null,
            authority: "https://idp.example.com/application/o/nebula/");

        var config = BuildConfig(authority: "https://idp.example.com/application/o/nebula/");
        var factory = new StubHttpClientFactory(messageHandler);

        // Act
        var result = await Nebula.Api.Endpoints.AuthEndpoints.LogoutAsync(httpContext, config, factory, NullLogger<Program>.Instance, CancellationToken.None);

        // Assert — 204 returned
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();

        // Assert — no HTTP call to authentik was made
        messageHandler.Requests.Should().BeEmpty();

        // Assert — Set-Cookie clear header is still emitted
        AssertClearCookieAppended(httpContext);
    }

    // ------------------------------------------------------------------
    // No authority configured (Development mode) — skip revocation silently
    // ------------------------------------------------------------------

    [Fact]
    public async Task Logout_NoAuthorityConfigured_SkipsRevocationAndReturns204()
    {
        // Arrange — empty configuration (dev mode has no Authority)
        var (httpContext, messageHandler) = BuildContext(
            refreshTokenCookieValue: "some-dev-token",
            authority: null);

        var config = BuildConfig(authority: null);
        var factory = new StubHttpClientFactory(messageHandler);

        // Act
        var result = await Nebula.Api.Endpoints.AuthEndpoints.LogoutAsync(httpContext, config, factory, NullLogger<Program>.Instance, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
        messageHandler.Requests.Should().BeEmpty(); // no HTTP call
        AssertClearCookieAppended(httpContext);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static (DefaultHttpContext httpContext, CapturingMessageHandler messageHandler) BuildContext(
        string? refreshTokenCookieValue,
        HttpStatusCode revocationStatusCode = HttpStatusCode.OK,
        Exception? throwOnSend = null,
        string? authority = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new System.IO.MemoryStream();

        if (refreshTokenCookieValue is not null)
        {
            httpContext.Request.Headers.Cookie = $"{Nebula.Api.Endpoints.AuthEndpoints.RefreshTokenCookieName}={refreshTokenCookieValue}";
        }

        var messageHandler = throwOnSend is not null
            ? new CapturingMessageHandler(throwOnSend)
            : new CapturingMessageHandler(revocationStatusCode);

        return (httpContext, messageHandler);
    }

    private static IConfiguration BuildConfig(string? authority)
    {
        var data = new Dictionary<string, string?>();
        if (authority is not null)
            data["Authentication:Authority"] = authority;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    private static void AssertClearCookieAppended(DefaultHttpContext httpContext)
    {
        // The response must contain a Set-Cookie header that zeroes out the refresh_token.
        var setCookieHeaders = httpContext.Response.Headers["Set-Cookie"];
        setCookieHeaders.Should().NotBeEmpty(
            because: "logout must always clear the refresh_token cookie");

        var setCookieValue = setCookieHeaders.ToString();
        setCookieValue.Should().Contain(Nebula.Api.Endpoints.AuthEndpoints.RefreshTokenCookieName,
            because: "the cleared cookie must target the refresh_token name");
        setCookieValue.Should().Contain("max-age=0",
            because: "Max-Age=0 instructs the browser to delete the cookie immediately");
        setCookieValue.Should().ContainEquivalentOf("httponly",
            because: "the cookie must remain HttpOnly");
        setCookieValue.Should().ContainEquivalentOf("secure",
            because: "the cookie must remain Secure");
        setCookieValue.Should().ContainEquivalentOf("samesite=strict",
            because: "the cookie must remain SameSite=Strict");
        setCookieValue.Should().Contain("path=/",
            because: "the cookie must be scoped to the root path");
    }
}

// ---------------------------------------------------------------------------
// Test infrastructure — stub HttpClientFactory + capturing message handler
// ---------------------------------------------------------------------------

internal sealed class CapturingMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly Exception? _exception;

    public List<HttpRequestMessage> Requests { get; } = new();

    public CapturingMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
    }

    public CapturingMessageHandler(Exception exception)
    {
        _exception = exception;
        _statusCode = HttpStatusCode.OK; // unused
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_exception is not null)
            throw _exception;

        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}

internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly CapturingMessageHandler _handler;

    public StubHttpClientFactory(CapturingMessageHandler handler) => _handler = handler;

    public HttpClient CreateClient(string name) => new(_handler);
}
