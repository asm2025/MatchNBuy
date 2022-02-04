using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace MatchNBuy.Model;

[DebuggerDisplay("{Name}")]
[Serializable]
public class Interest : IEntity
{
	private string _name;

	[Key]
	public Guid Id { get; set; }

	[Required]
	[StringLength(255)]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}

	public virtual ICollection<UserInterest> UserInterests { get; set; }
}