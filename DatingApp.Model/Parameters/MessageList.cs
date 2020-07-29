using System;

namespace DatingApp.Model.Parameters
{
	[Serializable]
	public class MessageList : UserSortablePagination
	{
		public string MessageContainer { get; set; } = "Unread";
	}
}