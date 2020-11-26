using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.DateTime;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using MatchNBuy.Model;
using MatchNBuy.Model.Parameters;
using JetBrains.Annotations;
using MatchNBuy.Model.TransferObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Thread = MatchNBuy.Model.Thread;

namespace MatchNBuy.Data.Repositories
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

		/// <inheritdoc />
		public IQueryable<Message> List(string userId, IPagination settings = null)
		{
			ThrowIfDisposed();

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
											.Where(e => !e.IsArchived && e.ThreadId == null && e.MessageSent >= range.Start && e.MessageSent <= range.End);
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
		public async Task<IList<Message>> ListAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Message> queryable = List(userId, settings);
			return await queryable.ToListAsync(token);
		}

		/// <inheritdoc />
		public Message ReplyTo(string userId, Message message, MessageToEdit messageParam)
		{
			ThrowIfDisposed();
			if (message.SenderId != userId && message.RecipientId != userId) throw new UnauthorizedAccessException("The sender must be a part of the conversation.");

			Thread thread;
			
			if (message.ThreadId == null)
			{
				thread = new Thread
				{
					Id = Guid.NewGuid().ToString(),
					Subject = message.Subject,
					SenderId = message.SenderId
				};
			}
			else
			{
				thread = Context.Threads.FirstOrDefault(e => e.Id == message.ThreadId);
				if (thread == null) return null;
				thread.IsArchived = false;
				Context.Update(thread);
			}

			message.IsArchived = false;
			Context.Update(message);

			Message newMessage = new Message
			{
				Id = Guid.NewGuid(),
				ThreadId = thread.Id,
				SenderId = userId,
				RecipientId = message.SenderId,
				Subject = message.Subject,
				Content = messageParam.Content,
				MessageSent = DateTime.UtcNow
			};

			DbSet.Add(newMessage);
			thread.Modified = DateTime.UtcNow;
			Context.SaveChanges();
			return newMessage;
		}

		/// <inheritdoc />
		public async Task<Message> ReplyToAsync(string userId, Message message, MessageToEdit messageParam, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (message.SenderId != userId && message.RecipientId != userId) throw new UnauthorizedAccessException("The sender must be a part of the conversation.");

			Thread thread;
			
			if (message.ThreadId == null)
			{
				thread = new Thread
				{
					Id = Guid.NewGuid().ToString(),
					Subject = message.Subject,
					SenderId = message.SenderId
				};
			}
			else
			{
				thread = await Context.Threads.FirstOrDefaultAsync(e => e.Id == message.ThreadId, token);
				if (thread == null) return null;
				thread.IsArchived = false;
				Context.Update(thread);
			}

			message.IsArchived = false;
			Context.Update(message);

			Message newMessage = new Message
			{
				Id = Guid.NewGuid(),
				ThreadId = thread.Id,
				Thread = thread,
				SenderId = userId,
				RecipientId = message.SenderId,
				Subject = message.Subject,
				Content = messageParam.Content,
				MessageSent = DateTime.UtcNow
			};

			await DbSet.AddAsync(newMessage, token);
			thread.Modified = DateTime.UtcNow;
			await Context.SaveChangesAsync(token);
			return newMessage;
		}

		/// <inheritdoc />
		public void Archive(Message message)
		{
			ThrowIfDisposed();
			if (message.IsArchived) return;
			message.IsArchived = true;
			Context.Update(message);
			Context.SaveChanges();
		}

		/// <inheritdoc />
		public async Task ArchiveAsync(Message message, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (message.IsArchived) return;
			message.IsArchived = true;
			Context.Update(message);
			await Context.SaveChangesAsync(token);
		}

		/// <inheritdoc />
		public IQueryable<Thread> Threads(string userId, IPagination settings = null)
		{
			ThrowIfDisposed();
			settings ??= new MessageList();

			DateTimeRange range = settings is MessageList messageList
									? DateTimeHelper.GetRange(messageList.FromDate, messageList.ToDate)
									: DateTimeHelper.GetRange(null, null);
			IQueryable<Thread> queryable = Context.Threads
											.Include(e => e.Messages)
											.Where(e => !e.IsArchived && (e.RecipientId == userId || e.SenderId == userId) && e.Modified >= range.Start && e.Modified <= range.End);

			ISortable sortable = settings as ISortable;

			if (sortable != null && (sortable.OrderBy == null || sortable.OrderBy.Count == 0))
			{
				sortable.OrderBy = new[]
				{
					new SortField(nameof(Model.Thread.Modified), SortType.Descending)
				};
			}

			if (settings is IIncludeSettings includeSettings && includeSettings.Include?.Count > 0)
			{
				queryable = includeSettings.Include.SkipNullOrEmpty()
											.Aggregate(queryable, (current, path) => current.Include(path));
			}
		
			if (sortable != null && sortable.OrderBy?.Count > 0)
			{
				bool addedFirst = false;

				foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
				{
					if (!addedFirst)
					{
						queryable = queryable.OrderBy(field.Name, field.Type);
						addedFirst = true;
						continue;
					}

					queryable = queryable.ThenBy(field.Name, field.Type);
				}
			}

			return queryable;
		}

		/// <inheritdoc />
		public async Task<IList<Thread>> ThreadsAsync(string userId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Thread> queryable = Threads(userId, settings);
			return await queryable.ToListAsync(token).As<List<Thread>, IList<Thread>>();
		}

		/// <inheritdoc />
		public IQueryable<Message> Thread(string threadId, IPagination settings = null)
		{
			ThrowIfDisposed();

			IQueryable<Message> queryable = DbSet
											.Include(e => e.Sender)
											.Include(e => e.Recipient)
											.Where(e => !e.IsArchived && e.ThreadId == threadId && (!e.RecipientDeleted || !e.SenderDeleted));
			
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
		public Task<IList<Message>> ThreadAsync(string threadId, IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IQueryable<Message> queryable = Thread(threadId, settings);
			return queryable.ToListAsync(token).As<List<Message>, IList<Message>>();
		}

		/// <inheritdoc />
		public void ArchiveThread(Thread thread)
		{
			ThrowIfDisposed();
			if (thread.IsArchived) return;
			thread.IsArchived = true;
			Context.Update(thread);

			foreach (Message message in Context.Messages.Where(e => e.ThreadId == thread.Id))
			{
				message.IsArchived = true;
				Context.Update(message);
			}

			Context.SaveChanges();
		}

		/// <inheritdoc />
		public async Task ArchiveThreadAsync(Thread thread, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (thread.IsArchived) return;
			thread.IsArchived = true;
			Context.Update(thread);

			foreach (Message message in Context.Messages.Where(e => e.ThreadId == thread.Id))
			{
				message.IsArchived = true;
				Context.Update(message);
			}

			await Context.SaveChangesAsync(token);
		}
	}
}