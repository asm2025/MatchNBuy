using System;
using asm.Patterns.Pagination;

namespace DatingApp.Model.Parameters
{
	[Serializable]
	public class UserSortablePagination : SortablePagination
	{
		public string UserId { get; set; }
	}
}