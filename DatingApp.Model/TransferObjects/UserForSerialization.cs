using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{UserName}, {Email}, {FirstName} {LastName}")]
	public class UserForSerialization
    {
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string KnownAs { get; set; }
		public Genders Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
		public Guid CityId { get; set; }
    }
}