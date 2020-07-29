using System;
using asm.Extensions;

namespace DatingApp.Model.Parameters
{
	[Serializable]
	public class UserList : UserSortablePagination
	{
		private int? _minAge;
		private int? _maxAge;

		public Genders? Gender { get; set; }

		public int? MinAge
		{
			get => _minAge;
			set
			{
				_minAge = value?.Within(User.AGE_MIN, User.AGE_MAX);
				if (MaxAge < _minAge) MaxAge = _minAge;
			}
		}

		public int? MaxAge
		{
			get => _maxAge;
			set
			{
				_maxAge = value?.Within(MinAge ?? User.AGE_MIN, User.AGE_MAX);
				if (MinAge > _maxAge) MinAge = _maxAge;
			}
		}

		public bool Likees { get; set; }
		
		public bool Likers { get; set; }
	}
}