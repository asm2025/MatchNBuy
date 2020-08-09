using System;
using System.ComponentModel.DataAnnotations;
using asm.Data.Model;

namespace MatchNBuy.Model
{
	[Serializable]
	public class Like : IEntity
	{
		[Key]
		public Guid Id { get; set; }
		public string LikerId { get; set; }
		public virtual User Liker { get; set; }
		public string LikeeId { get; set; }
		public virtual User Likee { get; set; }
	}
}