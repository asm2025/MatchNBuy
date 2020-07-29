using System;
using asm.Data.Model;

namespace DatingApp.Model
{
	[Serializable]
	public class UserInterest : IEntity
	{
		public string UserId { get; set; }
		public virtual User User { get; set; }
		public Guid InterestId { get; set; }
		public virtual Interest Interest { get; set; }
	}
}