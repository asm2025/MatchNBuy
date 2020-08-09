using asm.Core.Data.Entity.Patterns.Repository;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Repositories
{
	public interface ICountryRepositoryBase : IRepositoryBase<DataContext, Country>
	{
	}
}