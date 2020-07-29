using System;
using System.ComponentModel.DataAnnotations;
using asm.Data.Model;

namespace DatingApp.Model
{
	[Serializable]
    public class Photo : IEntity
    {
        [Key]
        public Guid Id { get; set; }
		[Required]
        [StringLength(255)]
        public string Url { get; set; }
		[StringLength(512)]
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsDefault { get; set; }
        [Required]
		public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}