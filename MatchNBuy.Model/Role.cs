using System;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace MatchNBuy.Model;

[DebuggerDisplay("{Name}")]
[Serializable]
public class Role : IdentityRole<string>, IEntity
{
	public const string Administrators = "Administrators";
	public const string Members = "Members";
		
	public static readonly string[] Roles = 
	{
		Administrators,
		Members
	};

	public virtual ICollection<UserRole> UserRoles { get; set; }
}