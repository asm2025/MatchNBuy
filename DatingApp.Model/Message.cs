using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using asm.Data.Model;
using asm.Extensions;

namespace DatingApp.Model
{
	[Serializable]
    public class Message : IEntity
	{
		private string _threadId;
		private string _senderId;
		private string _recipientId;

		[Key]
		public Guid Id { get; set; }

		[StringLength(256)]
		public string ThreadId
		{
			get => _threadId;
			set => _threadId = value;
		}

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

		[NotMapped]
		public bool IsRead => DateRead > DateTime.MinValue;

		private void UpdateThread()
		{
			if (string.IsNullOrEmpty(_threadId) && !string.IsNullOrEmpty(_senderId) && !string.IsNullOrEmpty(_recipientId))
			{
				_threadId = _senderId.IsLessThanOrEqual(_recipientId)
								? $"{_senderId}{_recipientId}"
								: $"{_recipientId}{_senderId}";
			}
			else
			{
				_threadId = null;
			}
		}
    }
}