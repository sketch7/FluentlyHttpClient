using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FluentlyHttpClient.Entity
{
	public class FluentHttpClientDbContext : DbContext
	{
		public FluentHttpClientDbContext(DbContextOptions options)
			: base(options)
		{ }

		public DbSet<HttpResponseEntity> HttpResponses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		public Task Initialize()
		{
			return Database.MigrateAsync();
		}

		public Task Commit()
		{
			return SaveChangesAsync();
		}
	}
}