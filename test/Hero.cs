using System.ComponentModel;

namespace FluentlyHttpClient.Test
{
	/// <summary>
	/// Organization model which is used for tests.
	/// </summary>
	public class Hero
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
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
}