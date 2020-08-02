using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Extensions;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using DatingApp.Model;
using DatingApp.Model.Parameters;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class MessageRepository : Repository<DataContext, Message>, IMessageRepository
	{
		private static readonly Lazy<PropertyInfo[]> __keyProperties = new Lazy<PropertyInfo[]>(() => new[] { typeof(Message).GetProperty(nameof(Message.Id))}, LazyThreadSafetyMode.PublicationOnly);

		/// <inheritdoc />
		public MessageRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<MessageRepository> logger)
			: base(context, configuration, logger)
		{
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;

		public IQueryable<Message> ListByUser(string userId, IPagination settings = null)
		{
			ThrowIfDisposed();
			
			MessageContainers container = settings is MessageList messageList
											? messageList.Container
											: MessageContainers.Default;
			IQueryable<Message> queryable = DbSet;

			switch (container)
			{
				case MessageContainers.Inbox:
					queryable = queryable.Where(e => e.RecipientId == userId && !e.RecipientDeleted);
					break;
				case MessageContainers.Outbox:
					queryable = queryable.Where(e => e.SenderId == userId && !e.SenderDeleted);
					break;
				default:
					queryable = queryable.Where(e => e.RecipientId == userId && !e.RecipientDeleted && !e.IsRead);
					break;
			}

			return PrepareListQuery(queryable, settings);
		}

		public Task<IList<Message>> ListByUserAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			settings ??= new Pagination();
			return ListByUser(userId, settings).Paginate(settings).ToListAsync(token).As<List<Message>, IList<Message>>();
		}

		public Paginated<MessageThread> ListThreads(string userId, IPagination settings = null)
		{
			ThrowIfDisposed();
			settings ??= new Pagination();
			
			var groups = DbSet
						.Include(e => e.Sender)
						.Include(e => e.Recipient)
						.Where(e => e.RecipientId == userId && !e.RecipientDeleted || e.SenderId == userId && !e.SenderDeleted)
						.GroupBy(e => new
						{
							e.SenderId,
							e.RecipientId
						});

			settings.Count = groups.Count();

			var scope = groups.Paginate(settings).ToList();
			List<MessageThread> threads = new List<MessageThread>();

			foreach (var item in scope)
			{
				Message message = item.FirstOrDefault();
				threads.Add(new MessageThread
				{
					SenderId = item.Key.SenderId,
					SenderKnownAs = message?.Sender.KnownAs,
					SenderPhotoUrl = message?.Sender.PhotoUrl,
					RecipientId = item.Key.RecipientId,
					RecipientKnownAs = message?.Recipient.KnownAs,
					RecipientPhotoUrl = message?.Recipient.PhotoUrl,
					Count = item.Count()
				});
			}

			return new Paginated<MessageThread>(threads, settings);
		}

		public async Task<Paginated<MessageThread>> ListThreadsAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			settings ??= new Pagination();

			var groups = from message in DbSet
						join sender in Context.Users on message.SenderId equals sender.Id
						join recipient in Context.Users on message.RecipientId equals recipient.Id
						where message.RecipientId == userId && !message.RecipientDeleted || message.SenderId == userId && !message.SenderDeleted
						group message by new
						{
							message.SenderId,
							message.RecipientId
						}
						into g
						select new
						{
							g.Key.SenderId,
							g.Key.RecipientId,
						};

			settings.Count = await groups.CountAsync(token);
			token.ThrowIfCancellationRequested();

			//List<MessageThread> threads = await groups.Paginate(settings)
			//									.Select(e => new MessageThread
			//									{
			//										SenderId = e.Key.SenderId,
			//										SenderKnownAs = e.First().Sender.KnownAs,
			//										SenderPhotoUrl = e.First().Sender.PhotoUrl,
			//										RecipientId = e.Key.RecipientId,
			//										RecipientKnownAs = e.First().Recipient.KnownAs,
			//										RecipientPhotoUrl = e.First().Recipient.PhotoUrl,
			//										Count = e.Count()
			//									})
			//									.ToListAsync(token);

			return new Paginated<MessageThread>(Enumerable.Empty<MessageThread>(), settings);
		}

		public IQueryable<Message> ListByThread(string userId, string recipientId, IPagination settings = null)
		{
			ThrowIfDisposed();

			IQueryable<Message> queryable = DbSet.Where(e => e.RecipientId == userId && e.SenderId == recipientId && !e.RecipientDeleted ||
															e.RecipientId == recipientId && e.SenderId == userId && !e.SenderDeleted);
			
			if (settings is ISortable sortable && (sortable.OrderBy == null || sortable.OrderBy.Count == 0))
			{
				sortable.OrderBy = new[]
				{
					new SortField(nameof(Message.MessageSent), SortType.Descending)
				};
			}

			return PrepareListQuery(queryable, settings);
		}

		public Task<IList<Message>> ListByThreadAsync(string userId, string recipientId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Message> queryable = ListByThread(userId, recipientId, settings);
			return queryable.ToListAsync(token).As<List<Message>, IList<Message>>();
		}
	}
}