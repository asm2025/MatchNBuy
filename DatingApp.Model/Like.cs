using System;
using asm.Data.Model;

namespace DatingApp.Model
{
	[Serializable]
	public class Like : IEntity
	{
		public string LikerId { get; set; }
		public virtual User Liker { get; set; }
		public string LikeeId { get; set; }
		public virtual User Likee { get; set; }
	}
}