using System;
using asm.Patterns.Imaging;
using JetBrains.Annotations;

namespace MatchNBuy.Model.ImageBuilders
{
	public interface IUserImageBuilder : IImageBuilder
	{
		[NotNull]
		string FileExtension { get; }

		Uri BuildRelative([NotNull] string imageName, ImageSize imageSize);
	}
}