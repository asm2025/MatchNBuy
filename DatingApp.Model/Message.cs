using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using asm.Data.Model;

namespace DatingApp.Model
{
	[Serializable]
    public class Message : IEntity
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(128)]
        public string SenderId { get; set; }
        public virtual User Sender { get; set; }

		[Required]
		[StringLength(128)]
        public string RecipientId { get; set; }
        public virtual User Recipient { get; set; }

		[Required]
        [StringLength(512, MinimumLength = 1)]
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }

		[NotMapped]
		public bool IsRead => DateRead > DateTime.MinValue;
    }
}