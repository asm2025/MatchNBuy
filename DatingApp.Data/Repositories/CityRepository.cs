using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class CityRepository : RepositoryBase<DataContext, City>, ICityRepositoryBase
	{
		/// <inheritdoc />
		public CityRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<CityRepository> logger)
			: base(context, configuration, logger)
		{
		}

		[NotNull]
		public IEnumerable<City> List([NotNull] string countryCode)
		{
			ThrowIfDisposed();
			countryCode = countryCode.Trim().ToUpperInvariant();
			if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
			return DbSet.Where(e => e.CountryCode == countryCode).ToList();
		}

		public ValueTask<IEnumerable<City>> ListAsync([NotNull] string countryCode, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			countryCode = countryCode.Trim().ToUpperInvariant();
			if (countryCode.Length == 0) throw new ArgumentNullException(nameof(countryCode));
			return new ValueTask<IEnumerable<City>>(DbSet.Where(e => e.CountryCode == countryCode));
		}
	}
}