using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace MatchNBuy.Model;

[Serializable]
public class Thread : IEntity
{
	[Key]
	[StringLength(128, MinimumLength = 128)]
	public string Id { get; set; }

	[Required]
	[StringLength(128, MinimumLength = 128)]
	public string SenderId { get; set; }

	public virtual User Sender { get; set; }

	[Required]
	[StringLength(128, MinimumLength = 128)]
	public string RecipientId { get; set; }

	public virtual User Recipient { get; set; }

	[Required]
	[StringLength(128)]
	public string Subject { get; set; }

	public DateTime Modified { get; set; }

	public bool IsArchived { get; set; }

	public virtual ICollection<Message> Messages { get; set; }
}