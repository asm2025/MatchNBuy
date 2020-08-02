using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("To: {RecipientId}")]
	public class MessageToAdd
	{
		[Required]
		public string RecipientId { get; set; }

		[Required]
		[StringLength(512, MinimumLength = 1)]
		public string Content { get; set; }
	}
}