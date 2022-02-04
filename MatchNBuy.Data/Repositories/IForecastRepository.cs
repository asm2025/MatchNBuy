using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Patterns.Repository;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Repositories;

public interface IForecastRepository : IRepositoryBase
{
	IList<Forecast> List(DateTime date);
	ValueTask<IList<Forecast>> ListAsync(DateTime date, CancellationToken token = default(CancellationToken));

	Forecast Get(DateTime date);
	ValueTask<Forecast> GetAsync(DateTime date);
	ValueTask<Forecast> GetAsync(DateTime date, CancellationToken token);
}