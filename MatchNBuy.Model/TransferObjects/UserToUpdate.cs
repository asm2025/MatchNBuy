using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using essentialMix.Extensions;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	[DebuggerDisplay("{FirstName} {LastName}")]
	public class UserToUpdate
	{
		private string _firstName;
		private string _knownAs;
		private string _lastName;

		[Required]
		[StringLength(255)]
		public string FirstName
		{
			get => _firstName;
			set => _firstName = value.ToNullIfEmpty();
		}

		[Required]
		[StringLength(255)]
		public string LastName
		{
			get => _lastName; 
			set => _lastName = value.ToNullIfEmpty();
		}

		[StringLength(255)]
		public string KnownAs
		{
			get => _knownAs ?? FirstName; 
			set => _knownAs = value.ToNullIfEmpty();
		}

		public Genders Gender { get; set; }
		
		[Phone]
		public string PhoneNumber { get; set; }

		[Required]
		public Guid CityId { get; set; }

		[StringLength(255)]
		public string Introduction { get; set; }

		[StringLength(255)]
		public string LookingFor { get; set; }
		
		public IList<string> Interests { get; set; }

		public IList<string> Roles { get; set; }
	}
}