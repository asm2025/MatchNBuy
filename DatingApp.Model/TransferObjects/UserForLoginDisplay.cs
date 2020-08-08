using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[DebuggerDisplay("[{KnownAs}] {FirstName} {LastName}")]
	[Serializable]
	public class UserForLoginDisplay
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string KnownAs { get; set; }
		public string PhotoUrl { get; set; }
	}
}