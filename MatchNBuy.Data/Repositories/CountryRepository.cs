using essentialMix.Core.Data.Entity.Patterns.Repository;
using JetBrains.Annotations;
using MatchNBuy.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.Data.Repositories
{
	public class CountryRepository : RepositoryBase<DataContext, Country, string>, ICountryRepositoryBase
	{
		/// <inheritdoc />
		public CountryRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<CityRepository> logger)
			: base(context, configuration, logger)
		{
		}
	}
}