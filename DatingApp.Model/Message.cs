using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using asm.Data.Model;

namespace DatingApp.Model
{
	[Serializable]
    public class Message : IEntity
	{
		private string _senderId;
		private string _recipientId;

		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(256)]
		public string ThreadId { get; protected set; }

		[Required]
		[StringLength(128)]
		public string SenderId
		{
			get => _senderId;
			set
			{
				_senderId = value;
				UpdateThread();
			}
		}

		public virtual User Sender { get; set; }

		[Required]
		[StringLength(128)]
		public string RecipientId
		{
			get => _recipientId;
			set
			{
				_recipientId = value;
				UpdateThread();
			}
		}

		public virtual User Recipient { get; set; }

		[Required]
        [StringLength(512, MinimumLength = 1)]
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
        public bool IsArchived { get; set; }

		private void UpdateThread()
		{
			if (_senderId == null || _recipientId == null) return;
			ThreadId = string.CompareOrdinal(_senderId, _recipientId) <= 0
							? $"{_senderId}{_recipientId}"
							: $"{_recipientId}{_senderId}";
		}
    }
}