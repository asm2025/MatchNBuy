using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{Name} [{CountryCode}]")]
	public class CityForList
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string CountryCode { get; set; }
	}
}