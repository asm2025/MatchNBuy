using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using JetBrains.Annotations;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Repositories;

public interface ICityRepositoryBase : IRepositoryBase<DataContext, City, Guid>
{
	IQueryable<City> List([NotNull] string countryCode);
	[NotNull]
	Task<IList<City>> ListAsync([NotNull] string countryCode, CancellationToken token = default(CancellationToken));
}