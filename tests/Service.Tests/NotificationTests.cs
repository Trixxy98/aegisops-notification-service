using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Service.Tests;

public sealed class NotificationTests : IClassFixture<WebApplicationFactory<Program>> {
    private readonly WebApplicationFactory<Program> _factory;

    public NotificationTests(WebApplicationFactory<Program> factory) {
        _factory = factory;
    }

    [Fact]
    public async Task Email_is_stored() {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/notifications", new {
            channel = "email",
            to = "ops@example.com",
            subject = "hello",
            body = "world",
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}