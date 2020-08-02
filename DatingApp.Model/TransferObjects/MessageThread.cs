using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[DebuggerDisplay("From: {SenderKnownAs}, To: {RecipientKnownAs}, Count = {Count}")]
	[Serializable]
	public class MessageThread
	{
		public string SenderId { get; set; }
		public string SenderKnownAs { get; set; }
		public string SenderPhotoUrl { get; set; }
		public string RecipientId { get; set; }
		public string RecipientKnownAs { get; set; }
		public string RecipientPhotoUrl { get; set; }
		public int Count { get; set; }
	}
}