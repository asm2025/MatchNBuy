using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using asm;
using asm.Threading.Collections.ProducerConsumer;
using asm.Extensions;
using asm.Helpers;
using asm.IO;
using asm.Newtonsoft.Helpers;
using asm.Threading.Helpers;
using AutoMapper;
using MatchNBuy.Data.Fakers;
using MatchNBuy.Model;
using MatchNBuy.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using asm.Logging.Helpers;
using Thread = MatchNBuy.Model.Thread;

namespace MatchNBuy.Data
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

		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<Country> Countries { get; set; }
		public DbSet<City> Cities { get; set; }
		public DbSet<Photo> Photos { get; set; }
		public DbSet<Interest> Interests { get; set; }
		public DbSet<UserInterest> UserInterests { get; set; }
		public DbSet<Like> Likes { get; set; }
		public DbSet<Thread> Threads { get; set; }
		public DbSet<Message> Messages { get; set; }

		/// <inheritdoc />
		protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
		{
			if (optionsBuilder.IsConfigured)
			{
				base.OnConfiguring(optionsBuilder);
				return;
			}

			string workingDirectory = AssemblyHelper.GetEntryAssembly().GetDirectoryPath();
			IHostEnvironment environment = new HostingEnvironment
			{
				EnvironmentName = EnvironmentHelper.GetEnvironmentName(),
				ApplicationName = AppDomain.CurrentDomain.FriendlyName,
				ContentRootPath = workingDirectory,
				ContentRootFileProvider = new PhysicalFileProvider(workingDirectory)
			};

			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(workingDirectory);

			IConfiguration configuration = IConfigurationBuilderHelper.CreateConfiguration()
																	.AddConfigurationFiles(EnvironmentHelper.GetEnvironmentName())
																	.AddEnvironmentVariables()
																	.AddUserSecrets()
																	.Build();
			IConfigurationSection dataSection = configuration.GetSection("data");
			bool enableLogging = dataSection.GetValue<bool>("logging");

			if (enableLogging)
			{
				optionsBuilder.UseLoggerFactory(LogFactoryHelper.ConsoleLoggerFactory)
							.EnableSensitiveDataLogging();
			}

			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseSqlite(dataSection.GetConnectionString("DefaultConnection"), e => e.MigrationsAssembly(typeof(DataContext).Assembly.GetName().Name));
			optionsBuilder.EnableDetailedErrors(environment.IsDevelopment());
		}

		/// <inheritdoc />
		protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

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

			modelBuilder.Entity<User>(user =>
			{
				user.OwnsMany(e => e.RefreshTokens, token =>
				{
					token.HasOne(e => e.User)
						.WithMany(e => e.RefreshTokens)
						.HasForeignKey(e => e.UserId)
						.OnDelete(DeleteBehavior.Cascade);

					token.HasIndex(e => e.Created);
					token.HasIndex(e => e.CreatedBy);
					token.HasIndex(e => e.Expires);
					token.HasIndex(e => e.Revoked);
					token.HasIndex(e => e.RevokedBy);
					token.HasIndex(e => e.ReplacedBy);
				});
			});

			modelBuilder.Entity<Country>(country =>
			{
				country.Property(e => e.Code)
						.HasConversion(e => e, s => s.ToUpper());

				country.HasMany(e => e.Cities)
						.WithOne(e => e.Country)
						.HasForeignKey(e => e.CountryCode)
						.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<City>(city =>
			{
				city.Property(e => e.CountryCode)
					.HasConversion(e => e, s => s.ToUpper());

				city.HasMany(e => e.Users)
					.WithOne(e => e.City)
					.HasForeignKey(e => e.CityId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Photo>(photo =>
			{
				photo.HasOne(e => e.User)
					.WithMany(e => e.Photos)
					.HasForeignKey(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				photo.HasIndex(e => e.DateAdded);
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
							.OnDelete(DeleteBehavior.Cascade);

				userInterest.HasOne(e => e.Interest)
							.WithMany(e => e.UserInterests)
							.HasForeignKey(e => e.InterestId)
							.OnDelete(DeleteBehavior.Cascade);
			});

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
					.OnDelete(DeleteBehavior.Cascade);

				like.HasOne(e => e.Liker)
					.WithMany(e => e.Likees)
					.HasForeignKey(e => e.LikerId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<Message>(message =>
			{
				message.HasOne(e => e.Sender)
						.WithMany(e => e.MessagesSent)
						.HasForeignKey(e => e.SenderId)
						.OnDelete(DeleteBehavior.Cascade);

				message.HasOne(u => u.Recipient)
						.WithMany(m => m.MessagesReceived)
						.HasForeignKey(e => e.RecipientId)
						.OnDelete(DeleteBehavior.Cascade);

				message.HasIndex(e => e.MessageSent);
				message.HasIndex(e => e.DateRead);
				message.HasIndex(e => e.IsArchived);
			});

			modelBuilder.Entity<Thread>(thread =>
			{
				thread.HasMany(e => e.Messages)
						.WithOne(e => e.Thread)
						.HasForeignKey(e => e.ThreadId)
						.OnDelete(DeleteBehavior.Cascade);

				thread.HasOne(e => e.Sender)
					.WithMany(e => e.ThreadsSent)
					.HasForeignKey(e => e.SenderId)
					.OnDelete(DeleteBehavior.Cascade);

				thread.HasOne(u => u.Recipient)
					.WithMany(m => m.ThreadsReceived)
					.HasForeignKey(e => e.RecipientId)
					.OnDelete(DeleteBehavior.Cascade);

				thread.HasIndex(e => e.Modified);
				thread.HasIndex(e => e.IsArchived);
			});
		}

		public async Task SeedData([NotNull] UserManager<User> userManager, [NotNull] RoleManager<Role> roleManager, [NotNull] string defaultPassword, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, [NotNull] IHostEnvironment environment, ILogger logger)
		{
			const string USER_SYNC_DATA = "UserSyncData.json";
			const int USERS_COUNT = 51;

			const string IMAGES_FOLDER_DEF = "files/images/users";
			// {0} = gender, {1} = number
			const string IMAGES_URL = "https://randomuser.me/api/portraits/{0}/{1}.jpg";
			const string IMAGES_GENDER_FEMALE = "women";
			const string IMAGES_GENDER_MALE = "men";
			const string IMAGES_PREFIX_FEMALE = "auto_f_";
			const string IMAGES_PREFIX_MALE = "auto_m_";

			List<Country> countries = await Countries.ToListAsync();
			if (countries.Count == 0) await SeedCountries(this, countries, logger);

			List<City> cities = await Cities.ToListAsync();
			if (cities.Count == 0) await SeedCities(this, countries, cities, logger);

			await SeedRoles(roleManager, logger);

			if (!await userManager.Users.AnyAsync())
			{
				IList<User> users = await SeedUsers(this, userManager, defaultPassword, cities, configuration, environment, logger);
				List<UserForSerialization> usersList = mapper.Map(users, new List<UserForSerialization>());
				string userData = JsonConvert.SerializeObject(new
				{
					Password = defaultPassword,
					Users = usersList
				}, JsonHelper.CreateSettings(true));
				await File.WriteAllTextAsync(USER_SYNC_DATA, userData);
			}

			static async Task SeedCountries(DataContext context, List<Country> countries, ILogger logger)
			{
				logger?.LogInformation("Adding countries data.");
				countries.AddRange(RegionInfoHelper.Regions.Values
													.OrderBy(e => e.EnglishName)
													.Where(e => !string.IsNullOrEmpty(e.ThreeLetterISORegionName))
													.Select(e => new Country
													{
														Code = e.ThreeLetterISORegionName,
														Name = e.EnglishName
													}));
				await context.Countries.AddRangeAsync(countries);
				await context.SaveChangesAsync();
				logger?.LogInformation($"Added {countries.Count} countries data.");
			}

			static async Task SeedCities(DataContext context, IReadOnlyCollection<Country> countries, List<City> cities, ILogger logger)
			{
				CityFaker cityFaker = new CityFaker();
				logger?.LogInformation("Adding cities data.");

				foreach (Country country in countries)
				{
					int n = RandomHelper.Next(5, 50);

					foreach (City city in cityFaker.GenerateLazy(n))
					{
						city.CountryCode = country.Code;
						city.Country = country;
						await context.Cities.AddAsync(city);
						cities.Add(city);
					}
				}

				await context.SaveChangesAsync();
				logger?.LogInformation($"Added {cities.Count} cities data.");
			}

			static async Task SeedRoles(RoleManager<Role> roleManager, ILogger logger)
			{
				int roles = 0;
				logger?.LogInformation("Adding roles data.");

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

					logger?.LogInformation($"Added '{roleName}' role.");
					roles++;
				}

				logger?.LogInformation($"Added {roles} roles.");
			}

			static async Task<IList<User>> SeedUsers(DataContext context, UserManager<User> userManager, string defaultPassword, IList<City> cities, IConfiguration configuration, IHostEnvironment environment, ILogger logger)
			{
				UserFaker usersFaker = new UserFaker(cities);
				IList<User> users = usersFaker.Generate(USERS_COUNT);
				IDictionary<Genders, Queue<string>> images = await DownloadUserImages(users, configuration, environment, logger);
				User admin = users[0];
				admin.UserName = "administrator";
				admin.Email = "admin@example.com";
				admin.EmailConfirmed = true;
				admin.PhoneNumberConfirmed = true;
				admin.LockoutEnabled = false;
				logger?.LogInformation("Adding users data.");

				int usersAdded = 0;

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
					logger?.LogInformation($"Adding '{user.UserName}' user data.");
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

					logger?.LogInformation($"Added '{user.UserName}' user data.");
					usersAdded++;
					result = userIsAdmin
								? await userManager.AddToRolesAsync(user, new[]
								{
									Role.Administrators,
									Role.Members
								})
								: await userManager.AddToRoleAsync(user, Role.Members);
					logger?.LogInformation($"Adding '{user.UserName}' to roles.");

					if (!result.Succeeded)
					{
						throw new DataException(result.Errors.Aggregate(new StringBuilder($"Could not add user '{user.UserName}' to role."),
																		(builder, error) => builder.AppendWithLine(error.Description),
																		builder => builder.ToString()));
					}

					logger?.LogInformation($"Added '{user.UserName}' to roles.");
					if (images == null || user.Gender == Genders.NotSpecified || !images.TryGetValue(user.Gender, out Queue<string> files)) continue;

					Photo photo = new Photo
					{
						UserId = user.Id,
						Url = files.Dequeue(),
						IsDefault = true,
						DateAdded = user.Created
					};
					await context.Photos.AddAsync(photo);
					user.Photos.Add(photo);
					logger?.LogInformation($"Assigned '{user.UserName}' a default image '{photo.Url}'.");
				}

				await context.SaveChangesAsync();
				logger?.LogInformation($"Added {usersAdded} users.");
				return users;
			}

			static async Task<IDictionary<Genders, Queue<string>>> DownloadUserImages(IList<User> users, IConfiguration configuration, IHostEnvironment environment, ILogger logger)
			{
				if (users.Count == 0) return null;

				string imagesUrl = UriHelper.ToUri(configuration.GetValue<string>("images:users:url"), UriKind.Relative).String() ?? IMAGES_FOLDER_DEF;
				string imagesPath = PathHelper.Trim(environment.ContentRootPath);
				if (string.IsNullOrEmpty(imagesPath) || !Directory.Exists(imagesPath)) imagesPath = Directory.GetCurrentDirectory();
				imagesPath = Path.Combine(imagesPath, imagesUrl.Replace('/', '\\').TrimStart('\\'));
				logger?.LogInformation($"Initialized images directory as '{imagesPath}'.");

				if (!DirectoryHelper.Ensure(imagesPath))
				{
					logger?.LogError($"Failed to create images directory '{imagesPath}'.");
					return null;
				}

				Queue<string> femalesNeeded = new Queue<string>();
				Queue<string> malesNeeded = new Queue<string>();
				Queue<string> females = new Queue<string>();
				Queue<string> males = new Queue<string>();
				string femalePattern = $"{IMAGES_PREFIX_FEMALE}??.jpg";
				string malePattern = $"{IMAGES_PREFIX_MALE}??.jpg";

				foreach (User user in users.Where(e => e.Gender == Genders.Female || e.Gender == Genders.Male))
				{
					string pattern;
					Queue<string> needed, queue;

					if (user.Gender == Genders.Female)
					{
						pattern = femalePattern;
						needed = femalesNeeded;
						queue = females;
					}
					else
					{
						pattern = malePattern;
						needed = malesNeeded;
						queue = males;
					}

					string path = Path.Combine(imagesPath, user.Id);

					if (!Directory.Exists(path))
					{
						needed.Enqueue(user.Id);
						continue;
					}

					string file = Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly)
											.FirstOrDefault();

					if (string.IsNullOrEmpty(file))
					{
						needed.Enqueue(user.Id);
						continue;
					}

					queue.Enqueue(file);
				}

				IDictionary<Genders, Queue<string>> result = new Dictionary<Genders, Queue<string>>
				{
					[Genders.Female] = females,
					[Genders.Male] = males
				};

				if (femalesNeeded.Count == 0 && malesNeeded.Count == 0) return result;
				logger?.LogInformation($"Will download {femalesNeeded.Count} female images and {malesNeeded.Count} male images.");

				Regex regex = new Regex("auto_[fm]_(?<x>\\d+)\\.jpg$", RegexHelper.OPTIONS_I);
				HashSet<int> usedFemales = new HashSet<int>(females.Select(e =>
				{
					Match match = regex.Match(e);
					return !match.Success
								? -1
								: int.Parse(match.Groups["x"].Value);
				}).Where(e => e > -1));
				HashSet<int> usedMales = new HashSet<int>(males.Select(e =>
				{
					Match match = regex.Match(e);
					return !match.Success
								? -1
								: int.Parse(match.Groups["x"].Value);
				}).Where(e => e > -1));

				// download will timeout in x minutes where x is a number between 0 and 10 from the configuration
				int timeout = configuration.GetValue("images:users:downloadTimeout", 5).Within(0, 10);

				// use multi-thread to download the images
				using (CancellationTokenSource cts = timeout > 0 ? new CancellationTokenSource(TimeSpan.FromMinutes(timeout)) : null)
				{
					CancellationToken token = cts?.Token ?? CancellationToken.None;
					IOHttpDownloadFileWebRequestSettings downloadSettings = new IOHttpDownloadFileWebRequestSettings
					{
						BufferSize = Constants.BUFFER_256_KB,
						Overwrite = true,
						Timeout = TimeSpan.FromSeconds(configuration.GetValue("images:users:requestTimeout", 30).Within(0, 180)).TotalIntMilliseconds()
					};

#if DEBUG
					int threads = configuration.GetValue<bool>("limitThreads")
									? 1
									: TaskHelper.ProcessMaximum;
#else
					int threads = TaskHelper.ProcessMaximum;
#endif
					// Imagine doing this from scratch!! I love my library!!
					using (IProducerConsumer<(string Id, Genders Gender, int Number)> requests = ProducerConsumerQueue.Create(ThreadQueueMode.Task, new ProducerConsumerThreadQueueOptions<(string Id, Genders Gender, int Number)>(threads, e =>
					{
						// copy to local vars for threading issues
						string path = Path.Combine(imagesPath, e.Id!);
						Queue<string> queue = result[e.Gender!];
						CancellationToken tkn = token;
						return DownloadUserImage(path, e.Gender, e.Number, queue, downloadSettings, logger, tkn);
					}), token))
					{
						requests.WorkStarted += (sender, args) => logger?.LogInformation($"Download started using {threads} threads...");
						requests.WorkCompleted += (sender, args) => logger?.LogInformation("Download completed.");

						int number;

						while (femalesNeeded.Count > 0)
						{
							do
							{
								number = RNGRandomHelper.Next(0, 99);
							}
							while (usedFemales.Contains(number));

							requests.Enqueue((femalesNeeded.Dequeue(), Genders.Female, number));
						}

						while (malesNeeded.Count > 0)
						{
							do
							{
								number = RNGRandomHelper.Next(0, 99);
							}
							while (usedMales.Contains(number));

							requests.Enqueue((malesNeeded.Dequeue(), Genders.Male, number));
						}

						requests.Complete();
						await requests.WaitAsync();
					}
				}

				return result;
			}

			static TaskResult DownloadUserImage(string sourcePath, Genders gender, int number, Queue<string> files, IOHttpDownloadFileWebRequestSettings settings, ILogger logger, CancellationToken token)
			{
				if (token.IsCancellationRequested) return TaskResult.Canceled;

				string url = string.Format(IMAGES_URL, gender == Genders.Female
															? IMAGES_GENDER_FEMALE
															: IMAGES_GENDER_MALE, number);
				FileStream stream = null;
				string fileName = Path.Combine(sourcePath, $"{(gender == Genders.Female ? IMAGES_PREFIX_FEMALE : IMAGES_PREFIX_MALE)}{number:D2}.jpg");

				try
				{
					// since this is already a thread/task, no need to use any async version.
					// This will automatically retry to download the file. I love my library!!
					stream = UriHelper.DownloadFile(url, fileName, settings);
					if (token.IsCancellationRequested) return TaskResult.Canceled;

					lock (files)
					{
						files.Enqueue(Path.GetFileName(fileName));
					}

					return TaskResult.Success;
				}
				catch (Exception e)
				{
					logger?.LogError(e.CollectMessages());
					return TaskResult.Error;
				}
				finally
				{
					ObjectHelper.Dispose(stream);
				}
			}
		}
	}
}
