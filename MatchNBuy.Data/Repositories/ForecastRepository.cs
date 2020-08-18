using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Data.Patterns.Parameters;
using asm.Data.Patterns.Repository;
using asm.Extensions;
using asm.Patterns.Pagination;
using MatchNBuy.Data.Fakers;
using MatchNBuy.Model;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.Data.Repositories
{
	public class ForecastRepository : RepositoryBase<Forecast>, IForecastRepository
	{
		private static readonly Lazy<PropertyInfo[]> __keyProperties = new Lazy<PropertyInfo[]>(() =>
		{
			return typeof(Forecast).GetProperties()
									.Where(e => e.HasAttribute<KeyAttribute>())
									.ToArray();
		}, LazyThreadSafetyMode.PublicationOnly);

		private readonly Lazy<ForecastFaker> _forecasts;

		/// <inheritdoc />
		public ForecastRepository([NotNull] IConfiguration configuration, ILogger<ForecastRepository> logger)
			: base(configuration, logger)
		{
			_forecasts = new Lazy<ForecastFaker>(() => new ForecastFaker());
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;

		/// <inheritdoc />
		[NotNull]
		protected override IQueryable<Forecast> ListInternal(IPagination settings = null)
		{
			settings ??= new Pagination();
			if (settings.PageSize < 1) settings.PageSize = 7;
			if (settings.Page < 1) settings.Page = 1;
			settings.Count = int.MaxValue;
			int start = (settings.Page - 1) * settings.PageSize;
			DateTime date = DateTime.Today;
			IList<Forecast> result = _forecasts.Value.Generate(settings.PageSize);
			result.ForEach((e, i) => e.Date = date.AddDays(start + i));
			return result.AsQueryable();
		}

		/// <inheritdoc />
		protected override ValueTask<IList<Forecast>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<IList<Forecast>>(ListInternal(settings).ToListAsync(token).As<List<Forecast>, IList<Forecast>>());
		}

		/// <inheritdoc />
		[NotNull]
		protected override Forecast GetInternal([NotNull] params object[] keys)
		{
			Forecast result = _forecasts.Value.Generate();
			result.Date = (DateTime)keys[0];
			return result;
		}

		/// <inheritdoc />
		protected override ValueTask<Forecast> GetAsyncInternal([NotNull] object[] keys, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<Forecast>(GetInternal(keys));
		}

		/// <inheritdoc />
		[NotNull]
		protected override Forecast GetInternal([NotNull] IGetSettings settings) { return GetInternal(settings.KeyValue); }

		/// <inheritdoc />
		protected override ValueTask<Forecast> GetAsyncInternal([NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<Forecast>(GetInternal(settings.KeyValue));
		}
	}
}