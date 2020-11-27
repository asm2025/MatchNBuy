using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{Token}")]
	public class RevokeTokenRequest
	{
		public string Token { get; set; }
	}
}