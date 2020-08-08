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
using AutoMapper;
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

		private readonly IMapper _mapper;

		/// <inheritdoc />
		public MessageRepository([NotNull] DataContext context, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, ILogger<MessageRepository> logger)
			: base(context, configuration, logger)
		{
			_mapper = mapper;
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;

		/// <inheritdoc />
		public IQueryable<Message> List(string userId, IPagination settings = null)
		{
			MessageContainers container = settings is MessageList messageList
											? messageList.Container
											: MessageContainers.Default;
			IQueryable<Message> queryable = DbSet
											.Include(e => e.Sender)
											.Include(e => e.Recipient)
											.Where(e => !e.IsArchived);
			switch (container)
			{
				case MessageContainers.Inbox:
					queryable = queryable.Where(e => e.RecipientId == userId && !e.RecipientDeleted);
					break;
				case MessageContainers.Outbox:
					queryable = queryable.Where(e => e.SenderId == userId && !e.SenderDeleted);
					break;
				default:
					queryable = queryable.Where(e => e.RecipientId == userId && !e.RecipientDeleted && e.DateRead == null);
					break;
			}

			return queryable;
		}

		/// <inheritdoc />
		public async Task<IList<Message>> ListAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Message> queryable = List(userId, settings);
			return await queryable.ToListAsync(token);
		}

		/// <inheritdoc />
		public Paginated<MessageThread> Threads(string userId, IPagination settings = null)
		{
			ThrowIfDisposed();
			settings ??= new MessageList();
			// todo
			return new Paginated<MessageThread>(Enumerable.Empty<MessageThread>(), settings);
		}

		/// <inheritdoc />
		public async Task<Paginated<MessageThread>> ThreadsAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			settings ??= new Pagination();

			// todo EF fucking core does not support anything meaningful at all!
			IQueryable<IGrouping<string, Message>> queryable = DbSet
																//.Include(e => e.Sender)
																//.Include(e => e.Recipient)
																//.Where(e => !e.IsArchived &&
																//			(e.RecipientId == userId && !e.RecipientDeleted || e.SenderId == userId && !e.SenderDeleted))
																.GroupBy(e => e.ThreadId);
			settings.Count = await queryable.CountAsync(token);
			
			IEnumerable<MessageThread> threads = settings.Count > 0
													? (await queryable.Paginate(settings).ToListAsync(token))
													.Select(e =>
													{
														MessageThread th = _mapper.Map<MessageThread>(e);
														if (th.Count == 0) return null;

														Message m = e.First(msg => msg.SenderId != userId || msg.RecipientId != userId);
														User participant = m.SenderId == userId
																				? m.Recipient
																				: m.Sender;
														th.Participant = _mapper.Map<UserForLoginDisplay>(participant);
														return th;
													})
													.Where(e => e != null)
													: Enumerable.Empty<MessageThread>();
			return new Paginated<MessageThread>(threads, settings);
		}

		/// <inheritdoc />
		public IQueryable<Message> Thread(string userId, string recipientId, IPagination settings = null)
		{
			ThrowIfDisposed();

			IQueryable<Message> queryable = DbSet.Where(e => !e.IsArchived &&
															(e.RecipientId == userId && e.SenderId == recipientId && !e.RecipientDeleted ||
															e.RecipientId == recipientId && e.SenderId == userId && !e.SenderDeleted));
			
			if (settings is ISortable sortable && (sortable.OrderBy == null || sortable.OrderBy.Count == 0))
			{
				sortable.OrderBy = new[]
				{
					new SortField(nameof(Message.MessageSent), SortType.Descending)
				};
			}

			return PrepareListQuery(queryable, settings);
		}

		/// <inheritdoc />
		public Task<IList<Message>> ThreadAsync(string userId, string recipientId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Message> queryable = Thread(userId, recipientId, settings);
			return queryable.ToListAsync(token).As<List<Message>, IList<Message>>();
		}

		/// <inheritdoc />
		public void ArchiveThread(string userId, string recipientId)
		{
			ThrowIfDisposed();

			IQueryable<Message> queryable = DbSet.Where(e => !e.IsArchived &&
															(e.RecipientId == userId && e.SenderId == recipientId ||
															e.RecipientId == recipientId && e.SenderId == userId));

			foreach (Message message in queryable)
			{
				message.IsArchived = true;
				Context.Entry(message).State = EntityState.Modified;
			}

			Context.SaveChanges();
		}

		/// <inheritdoc />
		public async Task ArchiveThreadAsync(string userId, string recipientId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			IQueryable<Message> queryable = DbSet.Where(e => !e.IsArchived &&
															(e.RecipientId == userId && e.SenderId == recipientId ||
															e.RecipientId == recipientId && e.SenderId == userId));

			await foreach (Message message in queryable.AsAsyncEnumerable().WithCancellation(token))
			{
				message.IsArchived = true;
				Context.Entry(message).State = EntityState.Modified;
			}

			await Context.SaveChangesAsync(token);
		}

		/// <inheritdoc />
		public void Archive(Message message)
		{
			if (message.IsArchived) return;
			message.IsArchived = true;
			Context.Entry(message).State = EntityState.Modified;
			Context.SaveChanges();
		}

		/// <inheritdoc />
		public async Task ArchiveAsync(Message message, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (message.IsArchived) return;
			message.IsArchived = true;
			Context.Entry(message).State = EntityState.Modified;
			await Context.SaveChangesAsync(token);
		}
	}
}