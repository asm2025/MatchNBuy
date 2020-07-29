using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;

namespace DatingApp.Data.Repositories
{
	public interface ILikeRepository : IRepository<DataContext, Like>
	{
	}
}