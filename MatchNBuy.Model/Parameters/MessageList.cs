using System;
using essentialMix.Patterns.Pagination;

namespace MatchNBuy.Model.Parameters
{
	[Serializable]
	public class MessageList : SortablePagination
	{
		public MessageContainers Container { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}