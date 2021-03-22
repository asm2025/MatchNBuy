using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using MatchNBuy.Model;
using JetBrains.Annotations;

namespace MatchNBuy.Data.Repositories
{
	public interface IPhotoRepository : IRepository<DataContext, Photo>
	{
		Photo GetDefault([NotNull] string userId);
		bool SetDefault([NotNull] Photo photo);
		Task<Photo> GetDefaultAsync([NotNull] string userId, CancellationToken token = default(CancellationToken));
		Task<bool> SetDefaultAsync([NotNull] Photo photo, CancellationToken token = default(CancellationToken));
	}
}