using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using DatingApp.Model;
using JetBrains.Annotations;

namespace DatingApp.Data.Repositories
{
	public interface IPhotoRepository : IRepository<DataContext, Photo>
	{
		Photo GetDefault([NotNull] string userId);
		bool SetDefault([NotNull] Photo photo);
		Task<Photo> GetDefaultAsync([NotNull] string userId, CancellationToken token = default(CancellationToken));
		Task<bool> SetDefaultAsync([NotNull] Photo photo, CancellationToken token = default(CancellationToken));
	}
}