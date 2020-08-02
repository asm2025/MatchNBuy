using System;
using asm.Patterns.Pagination;

namespace DatingApp.Model.Parameters
{
	[Serializable]
	public class MessageList : SortablePagination
	{
		public MessageContainers Container { get; set; }
	}
}