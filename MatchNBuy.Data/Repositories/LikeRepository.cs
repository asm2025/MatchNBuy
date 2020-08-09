using System;
using System.Reflection;
using System.Threading;
using asm.Core.Data.Entity.Patterns.Repository;
using MatchNBuy.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.Data.Repositories
{
	public class LikeRepository : Repository<DataContext, Like>, ILikeRepository
	{
		private static readonly Lazy<PropertyInfo[]> __keyProperties = new Lazy<PropertyInfo[]>(() => new[] { typeof(Like).GetProperty(nameof(Like.Id))}, LazyThreadSafetyMode.PublicationOnly);

		/// <inheritdoc />
		public LikeRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, ILogger<LikeRepository> logger)
			: base(context, configuration, logger)
		{
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;
	}
}