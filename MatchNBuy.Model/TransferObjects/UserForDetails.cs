using System;
using System.Collections.Generic;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class UserForDetails : UserForList
	{
		public string PhoneNumber { get; set; }
		public IList<string> Interests { get; set; }
		public IList<string> Roles { get; set; }
	}
}