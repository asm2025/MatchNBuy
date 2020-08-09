using System;
using System.Collections.Generic;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class UserForDetails : UserForList
	{
		public string City { get; set; }
		public string Country { get; set; }
		public IList<string> Interests { get; set; }
		public IList<string> Roles { get; set; }
	}
}