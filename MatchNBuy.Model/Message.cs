using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace MatchNBuy.Model
{
	[Serializable]
    public class Message : IEntity
	{
		[Key]
		public Guid Id { get; set; }

		[StringLength(128, MinimumLength = 128)]
		public string ThreadId { get; set; }

		public virtual Thread Thread { get; set; }

		[Required]
		[StringLength(128, MinimumLength = 128)]
		public string SenderId { get; set; }

		public virtual User Sender { get; set; }

		[Required]
		[StringLength(128, MinimumLength = 128)]
		public string RecipientId { get; set; }

		public virtual User Recipient { get; set; }

		[Required]
        [StringLength(128)]
        public string Subject { get; set; }

		[Required]
        [StringLength(512)]
        public string Content { get; set; }

        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
        public bool IsArchived { get; set; }
    }
}