using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Newtonsoft.Helpers;
using AutoMapper;
using DatingApp.Data.Fakers;
using DatingApp.Model;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatingApp.Data
{
	public class DataContext : IdentityDbContext<User, Role, string,
		IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
		IdentityRoleClaim<string>, IdentityUserToken<string>>
	{
		/// <inheritdoc />
		public DataContext()
		{
		}

		/// <inheritdoc />
		public DataContext(DbContextOptions options)
			: base(options)
		{
		}

		public DbSet<Country> Countries { get; set; }
		public DbSet<City> Cities { get; set; }
		public DbSet<Photo> Photos { get; set; }
		public DbSet<Interest> Interests { get; set; }
		public DbSet<UserInterest> UserInterests { get; set; }
		public DbSet<Like> Likes { get; set; }
		public DbSet<Message> Messages { get; set; }

		/// <inheritdoc />
		protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Country>()
					.HasMany(e => e.Cities)
					.WithOne(e => e.Country)
					.HasForeignKey(e => e.CountryCode)
					.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<City>()
					.HasMany(e => e.Users)
					.WithOne(e => e.City)
					.HasForeignKey(e => e.CityId)
					.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserRole>(userRole =>
			{
				userRole.HasKey(e => new
				{
					e.UserId,
					e.RoleId
				});

				userRole.HasOne(e => e.User)
							.WithMany(e => e.UserRoles)
							.HasForeignKey(e => e.UserId)
							.OnDelete(DeleteBehavior.Restrict);
				userRole.HasOne(e => e.Role)
							.WithMany(e => e.UserRoles)
							.HasForeignKey(e => e.RoleId)
							.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<UserInterest>(userInterest =>
			{
				userInterest.HasKey(e => new
				{
					e.UserId,
					e.InterestId
				});

				userInterest.HasOne(e => e.User)
							.WithMany(e => e.UserInterests)
							.HasForeignKey(e => e.UserId)
							.OnDelete(DeleteBehavior.Restrict);
				userInterest.HasOne(e => e.Interest)
							.WithMany(e => e.UserInterests)
							.HasForeignKey(e => e.InterestId)
							.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Photo>()
					.HasOne(e => e.User)
					.WithMany(e => e.Photos)
					.HasForeignKey(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Like>(like =>
			{
				like.HasKey(e => new
				{
					e.LikerId,
					e.LikeeId
				});

				like.HasOne(e => e.Likee)
					.WithMany(e => e.Likers)
					.HasForeignKey(e => e.LikeeId)
					.OnDelete(DeleteBehavior.Restrict);

				like.HasOne(e => e.Liker)
					.WithMany(e => e.Likees)
					.HasForeignKey(e => e.LikerId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Message>(message =>
			{
				message.HasKey(e => new
				{
					e.SenderId,
					e.RecipientId
				});

				message.HasOne(e => e.Sender)
						.WithMany(e => e.MessagesSent)
						.OnDelete(DeleteBehavior.Restrict);

				message.HasOne(u => u.Recipient)
						.WithMany(m => m.MessagesReceived)
						.OnDelete(DeleteBehavior.Restrict);
			});
		}

		public async Task SeedData([NotNull] UserManager<User> userManager, [NotNull] RoleManager<Role> roleManager, [NotNull] string defaultPassword, [NotNull] IMapper mapper)
		{
			const string USER_SYNC_DATA = "UserSyncData.json";

			if (!await Countries.AnyAsync())
			{
				await Countries.AddRangeAsync(RegionInfoHelper.Regions.Values
													.OrderBy(e => e.EnglishName)
													.Select(e => new Country
													{
														Code = e.ThreeLetterISORegionName,
														Name = e.EnglishName
													}));
				await SaveChangesAsync();
			}

			if (!await Cities.AnyAsync())
			{
				CityFaker cityFaker = new CityFaker();
				IList<Country> countries = await Countries.ToListAsync();

				foreach (Country country in countries)
				{
					int n = RandomHelper.Next(5, 50);

					foreach (City city in cityFaker.GenerateLazy(n))
					{
						city.CountryCode = country.Code;
						city.Country = country;
						await Cities.AddAsync(city);
					}
				}

				await SaveChangesAsync();
			}

			foreach (string roleName in Role.Roles)
			{
				if (await roleManager.RoleExistsAsync(roleName)) continue;
				IdentityResult result = await roleManager.CreateAsync(new Role
				{
					Id = Guid.NewGuid().ToString(),
					Name = roleName
				});

				if (!result.Succeeded)
				{
					throw new DataException(result.Errors.Aggregate(new StringBuilder($"Could not add role '{roleName}'."), 
																	(builder, error) => builder.AppendWithLine(error.Description), 
																	builder => builder.ToString()));
				}
			}

			if (!await userManager.Users.AnyAsync())
			{
				IList<City> cities = await Cities.ToListAsync();
				UserFaker usersFaker = new UserFaker(cities);
				IList<User> users = usersFaker.Generate(21);
				User admin = users[0];
				admin.UserName = "administrator";
				admin.Email = "admin@example.com";
				admin.EmailConfirmed = true;
				admin.PhoneNumberConfirmed = true;
				admin.DateOfBirth = DateTime.Today;
				admin.Gender = Genders.NotSpecified;
				admin.LockoutEnabled = false;

				for (int i = 0; i < users.Count; i++)
				{
					User user = users[i];
					User userInDb = await userManager.FindByNameAsync(user.UserName);

					if (userInDb != null)
					{
						users[i] = userInDb;
						continue;
					}

					bool userIsAdmin = user.UserName.IsSame("administrator");
					user.Created = DateTime.UtcNow;
					user.Modified = DateTime.UtcNow;
					user.LastActive = DateTime.UtcNow;
					IdentityResult result = await userManager.CreateAsync(user, defaultPassword);

					if (!result.Succeeded)
					{
						if (userIsAdmin)
						{
							throw new DataException(result.Errors.Aggregate(new StringBuilder($"Could not add admin user '{user.UserName}'."), 
																			(builder, error) => builder.AppendWithLine(error.Description), 
																			builder => builder.ToString()));
						}

						continue;
					}

					result = userIsAdmin
						? await userManager.AddToRolesAsync(user, new[]
						{
							Role.Administrators,
							Role.Members
						})
						: await userManager.AddToRoleAsync(user, Role.Members);

					if (!result.Succeeded)
					{
						throw new DataException(result.Errors.Aggregate(new StringBuilder($"Could not add user '{user.UserName}' to role."), 
																		(builder, error) => builder.AppendWithLine(error.Description), 
																		builder => builder.ToString()));
					}
				}

				List<UserForSerialization> usersList = mapper.Map(users, new List<UserForSerialization>());
				string userData = JsonConvert.SerializeObject(new
				{
					Password = defaultPassword,
					Users = usersList
				}, JsonHelper.CreateSettings(true));
				await File.WriteAllTextAsync(USER_SYNC_DATA, userData);
			}
		}
	}
}
