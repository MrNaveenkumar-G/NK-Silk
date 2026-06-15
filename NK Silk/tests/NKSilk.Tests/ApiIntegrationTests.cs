using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace NKSilk.Tests;

public class ApiIntegrationTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ApiIntegrationTests(TestAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Home_page_loads()
    {
        var resp = await _factory.CreateClient().GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/products")]
    [InlineData("/api/v1/categories")]
    [InlineData("/api/v1/offers")]
    public async Task Public_api_reads_return_ok(string path)
    {
        var resp = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Search_returns_results_and_facets()
    {
        // Match seeded casing — the in-memory provider's Contains is case-sensitive (SQL Server isn't).
        var resp = await _factory.CreateClient().GetAsync("/api/v1/search?q=Saree");
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 1);
        Assert.True(data.GetProperty("facets").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Sitemap_is_served_as_xml()
    {
        var resp = await _factory.CreateClient().GetAsync("/sitemap.xml");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("<urlset", body);
    }

    [Fact]
    public async Task Orders_api_requires_a_token()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/v1/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_issues_jwt_and_unlocks_orders()
    {
        var client = _factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@nksilk.com", password = "Admin@123" });
        login.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var orders = await client.GetAsync("/api/v1/orders");
        Assert.Equal(HttpStatusCode.OK, orders.StatusCode);
    }

    [Fact]
    public async Task Bad_login_is_rejected()
    {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@nksilk.com", password = "wrong-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
