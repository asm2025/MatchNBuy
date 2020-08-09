using System.ComponentModel.DataAnnotations;

namespace MatchNBuy.Model
{
	public enum Genders
	{
		[Display(Name = "Not Specified")]
		NotSpecified,
		Male,
		Female
	}
}