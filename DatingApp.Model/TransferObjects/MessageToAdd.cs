using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("From: {SenderId}, To: {RecipientId}")]
	public class MessageToAdd
	{
		public string SenderId { get; set; }

		public string RecipientId { get; set; }

		[Required]
		[StringLength(512)]
		public string Content { get; set; }
	}
}