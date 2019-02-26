using FluentHttpClient.Entity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluentHttpClient.Entity.Configurations
{
	public class HttpRequestMapping : IEntityTypeConfiguration<HttpRequest>
	{
		public void Configure(EntityTypeBuilder<HttpRequest> builder)
		{
			builder.ToTable(Constants.HttpRequestTable, Constants.SchemaName);

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Key).IsRequired().HasMaxLength(Constants.NormalTextLength);
			builder.Property(x => x.Method).IsRequired().HasMaxLength(Constants.ShortTextLength);
			builder.Property(x => x.Content).IsRequired();
			builder.Property(x => x.Headers).IsRequired();
			builder.Property(x => x.ResponseCode).IsRequired();
			builder.Property(x => x.Url).IsRequired().HasMaxLength(Constants.DefaultDomainLength);
		}
	}
}