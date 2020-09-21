using asm.Patterns.Images;
using JetBrains.Annotations;

namespace MatchNBuy.Model.ImageBuilders
{
	public interface IUserImageBuilder : IImageBuilder
	{
		[NotNull]
		string FileExtension { get; }

		string BuildRelative([NotNull] string imageName, ImageSize imageSize);
	}
}