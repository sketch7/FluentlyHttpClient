using System.ComponentModel.DataAnnotations;

namespace FluentlyHttpClient.Sample.Api.Heroes;

public record Hero
{
	[Required]
	public required string Key { get; init; }

	[Required]
	public required string Name { get; init; }
	public string? Title { get; init; }
}