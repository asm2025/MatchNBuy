using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using asm.Data.Model;
using asm.Extensions;
using Microsoft.AspNetCore.Identity;

namespace MatchNBuy.Model
{
	[DebuggerDisplay("User: {UserName}, E-mail:{Email}")]
	[Serializable]
	public class User : IdentityUser<string>, IEntity
    {
		public const int AGE_MIN = 16;
		public const int AGE_MAX = 70;
		public const string DATE_FORMAT = "yyyy-MM-dd";

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
        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime LastActive { get; set; }

		[StringLength(255)]
		public string Introduction { get; set; }
   
		[StringLength(255)]
		public string LookingFor { get; set; }
        
		[Required]
		public Guid CityId { get; set; }
		
		[StringLength(512)]
		public string Token { get; set; }

		[NotMapped]
		public string PhotoUrl => Photos?.FirstOrDefault(e => e.IsDefault)?.Url;

		public virtual City City { get; set; }

		public virtual ICollection<UserRole> UserRoles { get; set; }

		public virtual ICollection<UserInterest> UserInterests { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }

        public virtual ICollection<Like> Likers { get; set; }

        public virtual ICollection<Like> Likees { get; set; }

        public virtual ICollection<Thread> ThreadsSent { get; set; }

        public virtual ICollection<Thread> ThreadsReceived { get; set; }

        public virtual ICollection<Message> MessagesSent { get; set; }

        public virtual ICollection<Message> MessagesReceived { get; set; }
	}
}