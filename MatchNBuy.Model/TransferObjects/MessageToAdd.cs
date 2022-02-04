using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects;

[Serializable]
[DebuggerDisplay("To: {RecipientId}")]
public class MessageToAdd
{
	[Required]
	[StringLength(128, MinimumLength = 128)]
	public string RecipientId { get; set; }

	[Required]
	[StringLength(512)]
	public string Content { get; set; }
}