using System;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class MessageForList
	{
		public string ThreadId { get; set; }
		public string SenderId { get; set; }
		public string RecipientId { get; set; }
		public string Content { get; set; }
		public bool IsRead { get; set; }
		public DateTime? DateRead { get; set; }
		public DateTime MessageSent { get; set; }
	}
}