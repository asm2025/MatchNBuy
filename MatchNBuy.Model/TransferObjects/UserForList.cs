using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[DebuggerDisplay("[{KnownAs}] {FirstName} {LastName}")]
	[Serializable]
	public class UserForList : UserForLoginDisplay
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Introduction { get; set; }
		public string LookingFor { get; set; }
		public bool CanBeLiked { get; set; } = true;
		public bool CanBeDisliked { get; set; } = true;
		public int Likes { get; set; }
	}
}