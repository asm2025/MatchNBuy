using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using MatchNBuy.Model;
using JetBrains.Annotations;
using MatchNBuy.Model.TransferObjects;
using Thread = MatchNBuy.Model.Thread;

namespace MatchNBuy.Data.Repositories
{
	public interface IMessageRepository : IRepository<DataContext, Message>
	{
		[NotNull]
		IQueryable<Message> List([NotNull] string userId, IPagination settings = null);
		[ItemNotNull]
		Task<IList<Message>> ListAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		Message ReplyTo([NotNull] string userId, [NotNull] Message message, [NotNull] MessageToEdit messageParam);
		Task<Message> ReplyToAsync([NotNull] string userId, [NotNull] Message message, [NotNull] MessageToEdit messageParam, CancellationToken token = default(CancellationToken));
		void Archive([NotNull] Message message);
		Task ArchiveAsync([NotNull] Message message, CancellationToken token = default(CancellationToken));
		[NotNull]
		IQueryable<Thread> Threads([NotNull] string userId, IPagination settings = null);
		[ItemNotNull]
		Task<IList<Thread>> ThreadsAsync([NotNull] string userId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		[NotNull]
		IQueryable<Message> Thread([NotNull] string threadId, IPagination settings = null);
		Task<IList<Message>> ThreadAsync([NotNull] string threadId, IPagination settings = null, CancellationToken token = default(CancellationToken));
		void ArchiveThread([NotNull] Thread thread);
		Task ArchiveThreadAsync([NotNull] Thread thread, CancellationToken token = default(CancellationToken));
	}
}