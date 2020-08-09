using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[DebuggerDisplay("[{KnownAs}] {FirstName} {LastName}")]
	[Serializable]
	public class UserForList
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string KnownAs { get; set; }
		public Genders Gender { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Introduction { get; set; }
		public string LookingFor { get; set; }
		public Guid CityId { get; set; }
		public double Age { get; set; }
		public string PhotoUrl { get; set; }
	}
}