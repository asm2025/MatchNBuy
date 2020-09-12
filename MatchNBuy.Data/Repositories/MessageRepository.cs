using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.DateTime;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using asm.Threading.Extensions;
using AutoMapper;
using MatchNBuy.Model;
using MatchNBuy.Model.Parameters;
using MatchNBuy.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.Data.Repositories
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
			MessageContainers container;
			DateTimeRange range;

			if (settings is MessageList messageList)
			{
				container = messageList.Container;
				range = DateTimeHelper.GetRange(messageList.FromDate, messageList.ToDate);
			}
			else
			{
				container = MessageContainers.Default;
				range = DateTimeHelper.GetRange(null, null);
			}
			
			IQueryable<Message> queryable = DbSet
											.Include(e => e.Sender)
											.Include(e => e.Recipient)
											.Where(e => !e.IsArchived && e.MessageSent >= range.Start && e.MessageSent <= range.End);
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

			DateTimeRange range = settings is MessageList messageList
									? DateTimeHelper.GetRange(messageList.FromDate, messageList.ToDate)
									: DateTimeHelper.GetRange(null, null);
			IQueryable<Message> queryable = DbSet
											.Include(e => e.Sender)
											.Include(e => e.Recipient)
											.Where(e => !e.IsArchived && e.MessageSent >= range.Start && e.MessageSent <= range.End &&
														(e.RecipientId == userId && !e.RecipientDeleted || e.SenderId == userId && !e.SenderDeleted));
			settings.Count = queryable.GroupBy(e => e.ThreadId).Count();
			
			IEnumerable<MessageThread> threads = settings.Count > 0
													// the new EF Core does not support much for now so we have to do group by on the client
													// SQLite does not support stored procedures. I guess we have to do with what's available.
													? queryable.AsEnumerable()
																.GroupBy(e => e.ThreadId)
																.Paginate(settings)
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
		public async Task<Paginated<MessageThread>> ThreadsAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			settings ??= new MessageList();
			/*
			 * Sample .Net core 3.1 Web API back-end.\nTo test the users, some data has been seeded when this app first ran.\nFind the file .\\bin[\\Debug or Release\\...]\\UserSyncData.json for test data.
			 */
			DateTimeRange range = settings is MessageList messageList
									? DateTimeHelper.GetRange(messageList.FromDate, messageList.ToDate)
									: DateTimeHelper.GetRange(null, null);
			IQueryable<Message> queryable = DbSet
											.Include(e => e.Sender)
											.Include(e => e.Recipient)
											.Where(e => !e.IsArchived && e.MessageSent >= range.Start && e.MessageSent <= range.End &&
														(e.RecipientId == userId && !e.RecipientDeleted || e.SenderId == userId && !e.SenderDeleted));
			settings.Count = await queryable.GroupBy(e => e.ThreadId).CountAsync(token);
			token.ThrowIfCancellationRequested();
			
			IEnumerable<MessageThread> threads = settings.Count > 0
													// the new EF Core does not support much for now so we have to do group by on the client
													// SQLite does not support stored procedures. I guess we have to do with what's available.
													? queryable.AsEnumerable()
													.GroupBy(e => e.ThreadId)
													.Paginate(settings)
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