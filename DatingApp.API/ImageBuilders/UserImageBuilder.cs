using System;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Images;
using DatingApp.Data.ImageBuilders;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DatingApp.API.ImageBuilders
{
	public class UserImageBuilder : ImageBuilder, IUserImageBuilder
	{
		private readonly IHttpContextAccessor _context;

		/// <inheritdoc />
		public UserImageBuilder([NotNull] IConfiguration configuration, [NotNull] IHttpContextAccessor context)
			: base( UriHelper.Trim(configuration.GetValue<string>("images:users:url")))
		{
			_context = context;
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
			imageName = UriHelper.Trim(imageName) ?? throw new ArgumentNullException(nameof(imageName));
			return $"/{BaseUri}/{imageName}{FileExtension}";
		}
	}
}