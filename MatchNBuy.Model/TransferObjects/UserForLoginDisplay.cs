using System;
using System.Diagnostics;

namespace MatchNBuy.Model.TransferObjects;

[DebuggerDisplay("{KnownAs}")]
[Serializable]
public class UserForLoginDisplay
{
	public string Id { get; set; }
	public string UserName { get; set; }
	public string Email { get; set; }
	public string KnownAs { get; set; }
	public Genders Gender { get; set; }
	public DateTime DateOfBirth { get; set; }
	public double Age { get; set; }
	public string PhotoUrl { get; set; }
	public string CountryCode { get; set; }
	public string Country { get; set; }
	public Guid CityId { get; set; }
	public string City { get; set; }
	public DateTime Created { get; set; }
	public DateTime LastActive { get; set; }
}