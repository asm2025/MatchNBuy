using System;
using asm.Patterns.Imaging;
using JetBrains.Annotations;

namespace MatchNBuy.Model.ImageBuilders
{
	public interface IUserImageBuilder : IImageBuilder
	{
		[NotNull]
		string FileExtension { get; }

		string Build([NotNull] string id, string imageName, ImageSize imageSize = ImageSize.Default);

		string BuildRelative([NotNull] string id, string imageName, ImageSize imageSize = ImageSize.Default);
	}
}