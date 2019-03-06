using System.Reflection;
using System.Threading.Tasks;
using FluentlyHttpClient.Caching;
using Microsoft.EntityFrameworkCore;

namespace FluentHttpClient.Entity
{
	public class FluentHttpClientContext : DbContext
	{
		// dotnet ef migrations add Initial --project src/FluentHttpClient.Entity/FluentHttpClient.Entity.csproj --startup-project samples/FluentlyHttpClient.Sample.Api/FluentlyHttpClient.Sample.Api.csproj -c FluentHttpClientContext

		// dotnet ef database update --project src/FluentHttpClient.Entity/FluentHttpClient.Entity.csproj --startup-project samples/FluentlyHttpClient.Sample.Api/FluentlyHttpClient.Sample.Api.csproj -c FluentHttpClientContext

		public FluentHttpClientContext(DbContextOptions options)
			: base(options)
		{ }

		public DbSet<MessageItemStore> HttpResponses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		public Task Initialize()
		{
			return Database.MigrateAsync();
		}
	}
}