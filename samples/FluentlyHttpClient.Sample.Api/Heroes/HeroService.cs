namespace FluentlyHttpClient.Sample.Api.Heroes;

public interface IHeroService
{
	Task<ICollection<Hero>> GetAll();
	Task<Hero?> GetByKey(string key);
	Task Add(Hero input);
}

public class HeroService : IHeroService
{
	private readonly ICollection<Hero> _data =
	[
		new()
		{
			Key = "azmodan",
			Name = "Azmodan",
			Title = "Lord of Sin"
		},
		new()
		{
			Key = "rexxar",
			Name = "Rexxar",
			Title = "Champion of the Horde"
		},
		new()
		{
			Key = "maiev",
			Name = "Maiev",
			Title = "The Warden"
		},
		new()
		{
			Key = "malthael",
			Name = "Malthael",
			Title = "Aspect of Death"
		},
		new()
		{
			Key = "garrosh",
			Name = "Garrosh",
			Title = "Son of Hellscream"
		},
	];

	public Task<ICollection<Hero>> GetAll() => Task.FromResult(_data);

	public Task<Hero?> GetByKey(string key)
		=> Task.FromResult(_data.FirstOrDefault(x => x.Key == key));

	public async Task Add(Hero input)
	{
		var result = await GetByKey(input.Key);
		//if (result != null)
		//{
		//	throw new ApiException(HttpStatusCode.BadRequest)
		//	{
		//		ErrorCode = "error.hero.key-already-exists"
		//	};
		//}

		_data.Add(input);
	}
}