using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[DebuggerDisplay("From: {Sender.KnownAs}, To: {Recipient.KnownAs}, IsRead? {IsRead}")]
	[Serializable]
	public class MessageForList
	{
		public UserForLoginDisplay Sender { get; set; }
		public UserForLoginDisplay Recipient { get; set; }
		public string Content { get; set; }
		public bool IsRead { get; set; }
		public DateTime? DateRead { get; set; }
		public DateTime MessageSent { get; set; }
	}
}