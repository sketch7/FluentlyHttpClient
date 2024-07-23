using System.ComponentModel.DataAnnotations;

namespace FluentlyHttpClient.Sample.Api.Heroes;

public record Hero
{
	[Required]
	public string Key { get; set; }

	[Required]
	public string Name { get; set; }
	public string Title { get; set; }
}