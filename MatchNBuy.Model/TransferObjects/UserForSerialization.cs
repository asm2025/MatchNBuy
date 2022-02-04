using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects;

[Serializable]
[DebuggerDisplay("{UserName}, {Email}, {FirstName} {LastName}")]
public class UserForSerialization : UserForList
{
	public string PhoneNumber { get; set; }
	public DateTime Modified { get; set; }
}