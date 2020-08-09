using System;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Images;
using MatchNBuy.Data.ImageBuilders;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MatchNBuy.API.ImageBuilders
{
	public class WeatherImageBuilder : ImageBuilder, IWeatherImageBuilder
	{
		private readonly IHttpContextAccessor _context;
		private readonly string _extension;

		/// <inheritdoc />
		public WeatherImageBuilder([NotNull] IConfiguration configuration, [NotNull] IHttpContextAccessor context)
			: base( UriHelper.Trim(configuration.GetValue<string>("images:weather:url")))
		{
			_context = context;
			_extension = UriHelper.Trim(configuration.GetValue<string>("images:weather:extension")).Prefix('.');
		}

		/// <inheritdoc />
		public override Uri Build([NotNull] string imageName, ImageSize imageSize)
		{
			imageName = UriHelper.Trim(imageName) ?? throw new ArgumentNullException(nameof(imageName));
			HttpRequest request = _context.HttpContext.Request;
			return UriHelper.ToUri($"{request.Scheme}://{request.Host}/{BaseUri}/{imageName}{_extension}", UriKind.Absolute);
		}
	}
}
