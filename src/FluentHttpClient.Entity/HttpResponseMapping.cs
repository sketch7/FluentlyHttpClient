﻿using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace FluentHttpClient.Entity
{
	public class HttpResponseMapping : IEntityTypeConfiguration<HttpResponse>
	{
		public void Configure(EntityTypeBuilder<HttpResponse> builder)
		{
			builder.ToTable(Constants.HttpResponseTable, Constants.SchemaName);

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id).HasMaxLength(Constants.NormalTextLength);
			builder.Property(x => x.Name).HasMaxLength(Constants.NormalTextLength);
			builder.Property(x => x.Hash).IsRequired().HasMaxLength(Constants.LongTextLength);
			builder.Property(x => x.Url).IsRequired().HasMaxLength(Constants.LongTextLength);
			builder.Property(x => x.Content).IsRequired();
			builder.Property(x => x.StatusCode).IsRequired();
			builder.Property(x => x.ReasonPhrase).IsRequired().HasMaxLength(Constants.NormalTextLength);
			builder.Property(x => x.Version).IsRequired().HasMaxLength(Constants.ShortTextLength);
			builder.Property(x => x.ContentHeaders)
				.IsRequired()
				.HasMaxLength(Constants.LongTextLength)
				.HasConversion(FluentHttpHeadersConversion.Convert)
				;
			builder.Property(x => x.Headers)
				.IsRequired()
				.HasMaxLength(Constants.LongTextLength)
				.HasConversion(FluentHttpHeadersConversion.Convert)
				;
			builder.Property(x => x.RequestMessage)
				.IsRequired()
				.HasMaxLength(Constants.LongTextLength)
				.HasConversion(
				x => JsonConvert.SerializeObject(x),
				x => JsonConvert.DeserializeObject<HttpRequestMessage>(x));
		}
	}
}