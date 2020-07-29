using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;

namespace DatingApp.Data.Repositories
{
	public interface ICityRepositoryBase : IRepositoryBase<DataContext, City>
	{
	}
}