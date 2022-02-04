using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using MatchNBuy.Data.Fakers;
using MatchNBuy.Model;
using JetBrains.Annotations;

namespace MatchNBuy.Data.Repositories;

public class ForecastRepository : IForecastRepository
{
	private readonly Lazy<ForecastFaker> _forecasts;

	public ForecastRepository()
	{
		_forecasts = new Lazy<ForecastFaker>(() => new ForecastFaker());
	}

	/// <inheritdoc />
	[NotNull]
	public IList<Forecast> List(DateTime date)
	{
		date = date.ThisWeek().Start;
		IList<Forecast> result = _forecasts.Value.Generate(7);

		for (int i = 0; i < result.Count; i++)
			result[i].Date = date.AddDays(i);

		return result;
	}

	/// <inheritdoc />
	public ValueTask<IList<Forecast>> ListAsync(DateTime date, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<IList<Forecast>>(List(date));
	}

	/// <inheritdoc />
	[NotNull]
	public Forecast Get(DateTime date)
	{
		Forecast result = _forecasts.Value.Generate();
		result.Date = date;
		return result;
	}

	/// <inheritdoc />
	public ValueTask<Forecast> GetAsync(DateTime date) { return GetAsync(date, CancellationToken.None); }

	/// <inheritdoc />
	public ValueTask<Forecast> GetAsync(DateTime date, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<Forecast>(Get(date));
	}

	/// <inheritdoc />
	public void Dispose() { }
}