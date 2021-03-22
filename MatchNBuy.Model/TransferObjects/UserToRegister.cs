using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using essentialMix.ComponentModel.DataAnnotations;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{UserName}, {Email}, {FirstName} {LastName}")]
	public class UserToRegister
	{
		[Required]
		[UserName]
		[StringLength(128)]
		public string UserName { get; set; }
		
		[Required]
		[StringLength(32, MinimumLength = 6)]
		public string Password { get; set; }
		
		[Required]
		[EmailAddress]
		[StringLength(255)]
		public string Email { get; set; }
		
		[Phone]
		public string PhoneNumber { get; set; }
		
		[Required]
		[StringLength(255)]
		public string FirstName { get; set; }
		
		[StringLength(255)]
		public string LastName { get; set; }
		
		[StringLength(255)]
		public string KnownAs { get; set; }
		
		public Genders Gender { get; set; }
		
		public DateTime DateOfBirth { get; set; }
		
		public Guid CityId { get; set; }
		
		[StringLength(255)]
		public string Introduction { get; set; }
		
		[StringLength(255)]
		public string LookingFor { get; set; }
	}
}