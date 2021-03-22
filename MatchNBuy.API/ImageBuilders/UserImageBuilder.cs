using System;
using System.IO;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Imaging;
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
			: base(UriHelper.ToUri(configuration.GetValue<string>("images:users:url"), UriKind.Relative).String())
		{
			_context = context;
			FileExtension = PathHelper.Trim(configuration.GetValue<string>("images:users:extension")).Prefix('.');
		}

		public string FileExtension { get; }

		/// <inheritdoc />
		public sealed override Uri Build([NotNull] string imageName, ImageSize imageSize) { throw new NotSupportedException(); }

		/// <inheritdoc />
		public Uri Build(string id, string imageName, ImageSize imageSize = ImageSize.Default)
		{
			HttpRequest request = _context.HttpContext?.Request;
			if (request == null) return null;
			Uri relUrl = BuildRelative(id, imageName, imageSize);
			return relUrl == null
						? null
						: UriHelper.ToUri($"{request.Scheme}://{request.Host}{relUrl.String()}");
		}

		public Uri BuildRelative(string id, string imageName, ImageSize imageSize = ImageSize.Default)
		{
			imageName = UriHelper.Trim(imageName);
			if (string.IsNullOrEmpty(imageName)) return null;
			if (string.IsNullOrEmpty(Path.GetExtension(imageName))) imageName += FileExtension;
			return UriHelper.Combine(BaseUri, id, imageName);
		}
	}
}