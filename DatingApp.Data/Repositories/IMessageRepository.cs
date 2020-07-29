using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;

namespace DatingApp.Data.Repositories
{
	public interface IMessageRepository : IRepository<DataContext, Message>
	{
	}
}