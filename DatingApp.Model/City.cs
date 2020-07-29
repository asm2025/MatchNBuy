using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using asm.Data.Model;
using asm.Extensions;

namespace DatingApp.Model
{
	[DebuggerDisplay("{Name} [{CountryCode}]")]
	[Serializable]
	public class City : IEntity
	{
		private string _name;

		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(255)]
		public string Name
		{
			get => _name;
			set => _name = value.ToNullIfEmpty();
		}

		[Required]
		[StringLength(10)]
		public string CountryCode { get; set; }

		public virtual Country Country { get; set; }

		public virtual ICollection<User> Users { get; set; }
	}
}