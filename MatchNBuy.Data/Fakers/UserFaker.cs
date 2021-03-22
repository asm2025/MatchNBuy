using System;
using System.Collections.Generic;
using essentialMix.Helpers;
using Bogus;
using Bogus.DataSets;
using MatchNBuy.Model;
using JetBrains.Annotations;

namespace MatchNBuy.Data.Fakers
{
	public class UserFaker : Faker<User>
	{
		/// <inheritdoc />
		public UserFaker()
			: this(Array.Empty<City>())
		{
		}

		/// <inheritdoc />
		public UserFaker([NotNull] IList<City> cities)
		{
			base.RuleFor(e => e.Id, f => f.Random.Guid().ToString());
			base.RuleFor(e => e.Gender, () => (Genders)RNGRandomHelper.Next(1, 2));
			base.RuleFor(e => e.FirstName, (f, e) =>
			{
				switch (e.Gender)
				{
					case Genders.Male:
						return f.Name.FirstName(Name.Gender.Male);
					case Genders.Female:
						return f.Name.FirstName(Name.Gender.Female);
					default:
						return f.Name.FirstName();
				}
			});
			base.RuleFor(e => e.LastName, f => f.Person.LastName);
			base.RuleFor(e => e.Email, (f, e) => f.Internet.ExampleEmail(e.FirstName, e.LastName));
			base.RuleFor(e => e.PhoneNumber, f => f.Person.Phone);
			base.RuleFor(e => e.KnownAs, (f, e) => e.FirstName);
			base.RuleFor(e => e.UserName, (f, e) => f.Internet.UserName(e.FirstName, e.LastName));
			base.RuleFor(e => e.City, f => f.PickRandom(cities));
			base.RuleFor(e => e.CityId, (f, e) => e.City.Id);
			base.RuleFor(e => e.Created, f => f.Date.Past(RandomHelper.Next(1, 10)));
			base.RuleFor(e => e.DateOfBirth, f => f.Date.Past(RandomHelper.Next(16, 60), DateTime.Now.AddYears(-18)));
			base.RuleFor(e => e.Introduction, f => f.Lorem.Sentences());
			base.RuleFor(e => e.LookingFor, f => f.Lorem.Sentences());
			base.RuleFor(e => e.LastActive, f => f.Date.Past());
			base.RuleFor(e => e.EmailConfirmed, true);
			base.RuleFor(e => e.PhoneNumberConfirmed, true);
		}

		public int MaxSpecifiedGender { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override List<User> Generate(int count, string ruleSets = null)
		{
			List<User> users = base.Generate(count, ruleSets);
			int maxGender = MaxSpecifiedGender;
			if (maxGender <= 0 || users.Count <= maxGender) return users;
	
			int males = 0;
			int females = 0;

			foreach (User user in users)
			{
				switch (user.Gender)
				{
					case Genders.Male:
						if (males == maxGender) user.Gender = Genders.Unspecified;
						else males++;
						break;
					case Genders.Female:
						if (females == maxGender) user.Gender = Genders.Unspecified;
						else females++;
						break;
				}
			}

			return users;
		}
	}
}