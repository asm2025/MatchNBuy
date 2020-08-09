using System;
using asm.Patterns.Pagination;

namespace MatchNBuy.Model.Parameters
{
	[Serializable]
	public class MessageList : SortablePagination
	{
		public MessageContainers Container { get; set; }
	}
}