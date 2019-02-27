using System.Reflection;
using System.Threading.Tasks;
using FluentHttpClient.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1
{
	public class FluentHttpClientContext : DbContext
	{
		//public FluentHttpClientContext(DbContextOptions options)
		//	: base(options)
		//{ }

		public DbSet<HttpRequest> HttpRequests { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// dotnet ef migrations add InitialCreate --startup-project ../../ConsoleApp1/ConsoleApp1.csproj -c FluentHttpClientContext
			var connString = @"Data Source=.\SQLEXPRESS;Database=FluentHttpClient;Integrated Security=True";
			optionsBuilder.UseSqlServer(connString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			//var cascadeFKs = modelBuilder.Model.GetEntityTypes()
			//	.SelectMany(t => t.GetForeignKeys())
			//	.Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

			//foreach (var fk in cascadeFKs)
			//	fk.DeleteBehavior = DeleteBehavior.Restrict;

			//modelBuilder.ApplyConfiguration(new HttpRequestMapping());
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		public Task Initialize()
		{
			return Database.MigrateAsync();
		}

	}
}
