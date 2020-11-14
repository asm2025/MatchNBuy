using System;
using System.IO;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Imaging;
using JetBrains.Annotations;
using MatchNBuy.Model.ImageBuilders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MatchNBuy.API.ImageBuilders
{
	public class UserImageBuilder : ImageBuilder, IUserImageBuilder
	{
		private readonly IHttpContextAccessor _context;

		/// <inheritdoc />
		public UserImageBuilder([NotNull] IConfiguration configuration, [NotNull] IHttpContextAccessor context)
			: base(UriHelper.Trim(configuration.GetValue<string>("images:users:url")))
		{
			_context = context;
			FileExtension = UriHelper.Trim(configuration.GetValue<string>("images:users:extension")).Prefix('.');
		}

		public string FileExtension { get; }

		/// <inheritdoc />
		public sealed override Uri Build([NotNull] string imageName, ImageSize imageSize) => throw new NotSupportedException();

		/// <inheritdoc />
		public string Build(string id, string imageName, ImageSize imageSize = ImageSize.Default)
		{
			HttpRequest request = _context.HttpContext?.Request;
			if (request == null) return null;
			string relUrl = BuildRelative(imageName, id, imageSize);
			return string.IsNullOrEmpty(relUrl)
						? null
						: UriHelper.Combine($"{request.Scheme}://{request.Host}", relUrl)?.ToString();
		}

		/// <inheritdoc />
		public string BuildRelative(string id, string imageName, ImageSize imageSize = ImageSize.Default)
		{
			imageName = UriHelper.Trim(imageName);
			if (imageName == null) return null;
			if (string.IsNullOrEmpty(Path.GetExtension(imageName))) imageName += FileExtension;
			Uri uri = UriHelper.Join(BaseUri, id, imageName);
			return uri.PathAndQuery.TrimStart('/');
		}
	}
}