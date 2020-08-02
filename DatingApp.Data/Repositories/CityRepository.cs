using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Extensions;
using DatingApp.Model;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class CityRepository : RepositoryBase<DataContext, City>, ICityRepositoryBase
	{
		private static readonly Lazy<PropertyInfo[]> __keyProperties = new Lazy<PropertyInfo[]>(() => new[] { typeof(City).GetProperty(nameof(City.Id))}, LazyThreadSafetyMode.PublicationOnly);
		
		/// <inheritdoc />
		public CityRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<CityRepository> logger)
			: base(context, configuration, logger)
		{
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;

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
			return DbSet.Where(e => e.CountryCode == countryCode).ToListAsync(token).As<List<City>, IList<City>>();
		}
	}
}