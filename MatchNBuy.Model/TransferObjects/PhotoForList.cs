using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{Url}")]
	public class PhotoForList
	{
		public Guid Id { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public DateTime DateAdded { get; set; }
		public bool IsDefault { get; set; }
	}
}