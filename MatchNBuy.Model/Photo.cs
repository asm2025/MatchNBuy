using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace MatchNBuy.Model
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
		[StringLength(128, MinimumLength = 128)]
		public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}