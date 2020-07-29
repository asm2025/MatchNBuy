using System;
using System.Diagnostics;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{Name} [{Code}]")]
	public class CountryForList
	{
		public string Code { get; set; }
		public string Name { get; set; }
	}
}