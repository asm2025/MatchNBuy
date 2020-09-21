using System;
using System.IO;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Images;
using JetBrains.Annotations;
using MatchNBuy.Model.ImageBuilders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MatchNBuy.API.ImageBuilders
{
	public class UserImageBuilder : ImageBuilder, IUserImageBuilder
	{
		private readonly IHttpContextAccessor _context;
		private readonly string _default;

		/// <inheritdoc />
		public UserImageBuilder([NotNull] IConfiguration configuration, [NotNull] IHttpContextAccessor context)
			: base(UriHelper.Trim(configuration.GetValue<string>("images:users:url")))
		{
			_context = context;
			_default = UriHelper.Trim(configuration.GetValue<string>("images:users:default"));
			FileExtension = UriHelper.Trim(configuration.GetValue<string>("images:users:extension")).Prefix('.');
		}

		public string FileExtension { get; }

		/// <inheritdoc />
		public override Uri Build([NotNull] string imageName, ImageSize imageSize)
		{
			HttpRequest request = _context.HttpContext.Request;
			return UriHelper.ToUri($"{request.Scheme}://{request.Host}/{BuildRelative(imageName, imageSize)}", UriKind.Absolute);
		}

		[NotNull]
		public string BuildRelative(string imageName, ImageSize imageSize)
		{
			imageName = UriHelper.Trim(imageName) ?? _default ?? throw new ArgumentNullException(nameof(imageName));
			if (string.IsNullOrEmpty(Path.GetExtension(imageName))) imageName += FileExtension;
			return $"/{BaseUri}/{imageName}";
		}
	}
}