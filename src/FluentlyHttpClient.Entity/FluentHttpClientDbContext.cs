using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FluentlyHttpClient.Entity;

/// <summary>EF Core database context for the HTTP response cache store.</summary>
public class FluentHttpClientDbContext : DbContext
{
	/// <summary>Initializes a new instance of <see cref="FluentHttpClientDbContext"/>.</summary>
	public FluentHttpClientDbContext(DbContextOptions options)
		: base(options)
	{ }

	/// <summary>Gets the <see cref="HttpResponseEntity"/> data set.</summary>
	public DbSet<HttpResponseEntity> HttpResponses { get; set; } = null!;

	/// <inheritdoc />
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}

	/// <summary>Apply pending EF Core migrations idempotently.</summary>
	public Task Initialize() => Database.MigrateAsync();

	/// <summary>Persist pending changes to the database.</summary>
	public Task Commit() => SaveChangesAsync();
}