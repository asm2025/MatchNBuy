using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class MessageRepository : Repository<DataContext, Message>, IMessageRepository
	{
		/// <inheritdoc />
		public MessageRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<MessageRepository> logger)
			: base(context, configuration, logger)
		{
		}
	}
}