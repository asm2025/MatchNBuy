using System;
using System.ComponentModel.DataAnnotations;
using asm.Data.Model;

namespace MatchNBuy.Model
{
	[Serializable]
	public class UserInterest : IEntity
	{
		[Required]
		[StringLength(128, MinimumLength = 128)]
		public string UserId { get; set; }
		public virtual User User { get; set; }
		public Guid InterestId { get; set; }
		public virtual Interest Interest { get; set; }
	}
}