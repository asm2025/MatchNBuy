using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using DatingApp.Model;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatingApp.Data.Repositories
{
	public class PhotoRepository : Repository<DataContext, Photo>, IPhotoRepository
	{
		/// <inheritdoc />
		public PhotoRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<PhotoRepository> logger)
			: base(context, configuration, logger)
		{
		}

		/// <inheritdoc />
		public Photo GetDefault(string userId)
		{
			ThrowIfDisposed();
			return DbSet.FirstOrDefault(e => e.UserId == userId && e.IsDefault);
		}

		/// <inheritdoc />
		public Task<Photo> GetDefaultAsync(string userId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return DbSet.FirstOrDefaultAsync(e => e.UserId == userId && e.IsDefault, token);
		}

		/// <inheritdoc />
		protected override Photo AddInternal(Photo entity)
		{
			Photo photo = base.AddInternal(entity);
			UpdateDefaultPhotos(photo);
			return photo;
		}

		/// <inheritdoc />
		protected override async ValueTask<Photo> AddAsyncInternal(Photo entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			Photo photo = await base.AddAsyncInternal(entity, token);
			token.ThrowIfCancellationRequested();
			await UpdateDefaultPhotosAsync(photo, token);
			return photo;
		}

		/// <inheritdoc />
		protected override Photo UpdateInternal(Photo entity)
		{
			Photo photo = base.UpdateInternal(entity);
			UpdateDefaultPhotos(photo);
			return photo;
		}

		/// <inheritdoc />
		protected override async ValueTask<Photo> UpdateAsyncInternal(Photo entity, CancellationToken token = new CancellationToken())
		{
			token.ThrowIfCancellationRequested();
			Photo photo = await base.UpdateAsyncInternal(entity, token);
			token.ThrowIfCancellationRequested();
			await UpdateDefaultPhotosAsync(photo, token);
			return photo;
		}

		private void UpdateDefaultPhotos(Photo photo)
		{
			if (photo == null || !photo.IsDefault) return;
			DbSet.Where(e => e.UserId == photo.UserId && e.IsDefault)
				.ForEach(e =>
				{
					e.IsDefault = false;
					Context.Entry(e).State = EntityState.Modified;
				});
		}

		private ValueTask UpdateDefaultPhotosAsync(Photo photo, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (photo == null || !photo.IsDefault) return ValueTaskHelper.CompletedTask();
			return new ValueTask(DbSet.Where(e => e.UserId == photo.UserId && e.IsDefault)
				.ForEachAsync(e =>
				{
					e.IsDefault = false;
					Context.Entry(e).State = EntityState.Modified;
				}, token));
		}
	}
}