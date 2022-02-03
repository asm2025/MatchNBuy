using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using JetBrains.Annotations;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Repositories
{
	public interface IPhotoRepository : IRepository<DataContext, Photo, Guid>
	{
		Photo GetDefault([NotNull] string userId);
		bool SetDefault([NotNull] Photo photo);
		[NotNull]
		Task<Photo> GetDefaultAsync([NotNull] string userId, CancellationToken token = default(CancellationToken));
		[NotNull]
		Task<bool> SetDefaultAsync([NotNull] Photo photo, CancellationToken token = default(CancellationToken));
	}
}