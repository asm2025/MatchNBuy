using System;
using System.ComponentModel.DataAnnotations;
using asm.Data.Model;
using asm.Helpers;

namespace MatchNBuy.Model
{
	[Serializable]
    public class Message : IEntity
	{
		private string _senderId;
		private string _recipientId;

		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(128, MinimumLength = 128)]
		public string ThreadId { get; protected set; }

		[Required]
		[StringLength(128, MinimumLength = 128)]
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
		[StringLength(128, MinimumLength = 128)]
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
        [StringLength(512)]
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
        public bool IsArchived { get; set; }

		private void UpdateThread()
		{
			if (string.IsNullOrEmpty(_senderId) || string.IsNullOrEmpty(_recipientId))
			{
				ThreadId = null;
				return;
			}

			ThreadId = GuidHelper.Combine(_senderId, _recipientId).ToString("D");
		}
    }
}