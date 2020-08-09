using System;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class MessageThread
	{
		public string ThreadId { get; set; }
		public UserForLoginDisplay Participant { get; set; }
		public bool IsRead { get; set; }
		public DateTime LastModified { get; set; }
		public int Count { get; set; }
	}
}