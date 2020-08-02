using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;
using JetBrains.Annotations;

namespace DatingApp.Data.Repositories
{
	public interface ICityRepositoryBase : IRepositoryBase<DataContext, City>
	{
		IQueryable<City> List([NotNull] string countryCode);
		[NotNull]
		Task<IList<City>> ListAsync([NotNull] string countryCode, CancellationToken token = default(CancellationToken));
	}
}