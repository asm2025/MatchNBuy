using System.ComponentModel.DataAnnotations;

namespace DatingApp.Model
{
	public enum Genders
	{
		[Display(Name = "Not Specified")]
		NotSpecified,
		Male,
		Female
	}
}