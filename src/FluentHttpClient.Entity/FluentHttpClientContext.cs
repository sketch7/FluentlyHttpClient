using System.Threading.Tasks;
using FluentHttpClient.Entity.Configurations;
using FluentHttpClient.Entity.Models;
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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			//var cascadeFKs = modelBuilder.Model.GetEntityTypes()
			//	.SelectMany(t => t.GetForeignKeys())
			//	.Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

			//foreach (var fk in cascadeFKs)
			//	fk.DeleteBehavior = DeleteBehavior.Restrict;

			modelBuilder.ApplyConfiguration(new HttpRequestMapping());
		}

		public Task Initialize()
		{
			return Database.MigrateAsync();
		}

	}
}