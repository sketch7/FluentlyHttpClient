using System.ComponentModel;

namespace FluentlyHttpClient.Test;

public record Hero
{
	public string Key { get; set; } = null!;
	public string Name { get; set; } = null!;
	public string Title { get; set; } = null!;
}

public enum HeroRole
{
	[Description("assassin")]
	Assassin,

	[Description("fighter")]
	Fighter,

	[Description("warrior")]
	Warrior,
}