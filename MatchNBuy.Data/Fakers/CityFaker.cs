using Bogus;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Fakers;

public class CityFaker : Faker<City>
{
	/// <inheritdoc />
	public CityFaker()
	{
		base.RuleFor(e => e.Id, f => f.Random.Guid());
		base.RuleFor(e => e.Name, f => f.Address.City());
	}
}