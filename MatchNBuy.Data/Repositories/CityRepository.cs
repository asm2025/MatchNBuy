using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Extensions;
using JetBrains.Annotations;
using MatchNBuy.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.Data.Repositories;

public class CityRepository : RepositoryBase<DataContext, City, Guid>, ICityRepositoryBase
{
	/// <inheritdoc />
	public CityRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<CityRepository> logger)
		: base(context, configuration, logger)
	{
	}

	[NotNull]
	public IQueryable<City> List(string countryCode)
	{
		ThrowIfDisposed();
		countryCode = countryCode.Trim();
		if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
		return DbSet.Where(e => e.CountryCode == countryCode);
	}

	public Task<IList<City>> ListAsync(string countryCode, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		countryCode = countryCode.Trim();
		if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
		return DbSet.Where(e => e.CountryCode == countryCode).ToListAsync(token).As<List<City>, IList<City>>(token);
	}
}