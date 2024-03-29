﻿using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FluentlyHttpClient.Entity;

public class FluentHttpClientDbContext : DbContext
{
	public FluentHttpClientDbContext(DbContextOptions options)
		: base(options)
	{ }

	public DbSet<HttpResponseEntity> HttpResponses { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}

	public Task Initialize() => Database.MigrateAsync();

	public Task Commit() => SaveChangesAsync();
}