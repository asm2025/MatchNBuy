using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
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