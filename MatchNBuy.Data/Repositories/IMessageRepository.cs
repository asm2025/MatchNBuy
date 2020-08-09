using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Patterns.Pagination;
using MatchNBuy.Model;
using MatchNBuy.Model.TransferObjects;
using JetBrains.Annotations;

namespace MatchNBuy.Data.Repositories
{
	public interface IMessageRepository : IRepository<DataContext, Message>
	{
		[NotNull]
		IQueryable<Message> List([NotNull] string userId, IPagination settings = null);
		[ItemNotNull]
		Task<IList<Message>> ListAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		[NotNull]
		Paginated<MessageThread> Threads([NotNull] string userId, IPagination settings = null);
		[ItemNotNull]
		Task<Paginated<MessageThread>> ThreadsAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		[NotNull]
		IQueryable<Message> Thread([NotNull] string userId, [NotNull] string recipientId, IPagination settings = null);
		Task<IList<Message>> ThreadAsync([NotNull] string userId, [NotNull] string recipientId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		void ArchiveThread([NotNull] string userId, [NotNull] string recipientId);
		Task ArchiveThreadAsync([NotNull] string userId, [NotNull] string recipientId, CancellationToken token = default(CancellationToken));
		void Archive([NotNull] Message message);
		Task ArchiveAsync([NotNull] Message message, CancellationToken token = default(CancellationToken));
	}
}