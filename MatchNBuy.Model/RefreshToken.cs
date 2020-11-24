using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using asm.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace MatchNBuy.Model
{
	[Owned]
	[Serializable]
	public class RefreshToken : IEntity
	{
		[Key]
		[StringLength(90, MinimumLength = 80)]
		public string Value { get; set; }
		[Required]
		[StringLength(128, MinimumLength = 128)]
		public string UserId { get; set; }
		public virtual User User { get; set; }
		public DateTime Created { get; set; }
		public DateTime Expires { get; set; }
		[Required]
		public string CreatedBy { get; set; }
		public DateTime? Revoked { get; set; }
		public string RevokedBy { get; set; }
		public string ReplacedByToken { get; set; }

		[NotMapped]
		public bool IsExpired => DateTime.UtcNow >= Expires;

		[NotMapped]
		public bool IsActive => Revoked == null && !IsExpired;
	}
}