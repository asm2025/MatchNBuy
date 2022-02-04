using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects;

[Serializable]
[DebuggerDisplay("{Name} [{Code}]")]
public class CountryForList
{
	public string Code { get; set; }
	public string Name { get; set; }
}