using System;
using essentialMix.Patterns.Imaging;
using JetBrains.Annotations;

namespace MatchNBuy.Model.ImageBuilders;

public interface IUserImageBuilder : IImageBuilder
{
	[NotNull]
	string FileExtension { get; }

	Uri Build([NotNull] string id, string imageName, ImageSize imageSize = ImageSize.Default);

	Uri BuildRelative([NotNull] string id, string imageName, ImageSize imageSize = ImageSize.Default);
}