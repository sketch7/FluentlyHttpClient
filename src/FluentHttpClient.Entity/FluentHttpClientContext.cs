using Microsoft.EntityFrameworkCore;

namespace FluentHttpClient.Entity
{
	public class FluentHttpClientContext : DbContext
	{
		public FluentHttpClientContext(DbContextOptions options)
			: base(options)
		{ }

		public DbSet<HttpRequest> HttpRequests { get; set; }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	optionsBuilder.UseSqlServer(@"Server=(LocalDb)\MSSQLLocalDB;Database=FluentHttpClient;Integrated Security=True;");
		//}
	}
}