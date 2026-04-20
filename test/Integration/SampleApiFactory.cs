using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FluentlyHttpClient.Test.Integration;

/// <summary>
/// Shared WebApplicationFactory fixture for integration tests against the sample API.
/// Use via <c>IClassFixture&lt;SampleApiFactory&gt;</c>.
/// </summary>
public sealed class SampleApiFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
		=> builder.UseEnvironment("Testing");
}
