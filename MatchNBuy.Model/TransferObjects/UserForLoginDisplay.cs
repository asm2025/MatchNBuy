using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[DebuggerDisplay("[{KnownAs}] {FirstName} {LastName}")]
	[Serializable]
	public class UserForLoginDisplay
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string KnownAs { get; set; }
		public Genders Gender { get; set; }
		public string PhotoUrl { get; set; }
		public DateTime DateOfBirth { get; set; }
		public Guid CityId { get; set; }
	}
}