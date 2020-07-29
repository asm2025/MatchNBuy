using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class LikeRepository : Repository<DataContext, Like>, ILikeRepository
	{
		/// <inheritdoc />
		public LikeRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<LikeRepository> logger)
			: base(context, configuration, logger)
		{
		}
	}
}