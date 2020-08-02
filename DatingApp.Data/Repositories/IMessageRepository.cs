using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Patterns.Pagination;
using DatingApp.Model;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;

namespace DatingApp.Data.Repositories
{
	public interface IMessageRepository : IRepository<DataContext, Message>
	{
		[NotNull]
		IQueryable<Message> ListByUser([NotNull] string userId, IPagination settings = null);
		Task<IList<Message>> ListByUserAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		[NotNull]
		Paginated<MessageThread> ListThreads([NotNull] string userId, IPagination settings = null);
		[ItemNotNull]
		Task<Paginated<MessageThread>> ListThreadsAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		[NotNull]
		IQueryable<Message> ListByThread([NotNull] string userId, [NotNull] string recipientId, IPagination settings = null);
		Task<IList<Message>> ListByThreadAsync([NotNull] string userId, [NotNull] string recipientId, IPagination settings = null, CancellationToken token = default(CancellationToken));
	}
}