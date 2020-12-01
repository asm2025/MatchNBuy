using System;
using System.Security.Claims;
using MatchNBuy.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using asm.Core.Web.Middleware;
using Serilog;
using asm.Newtonsoft.Helpers;
using asm.Data.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using asm.Logging.Helpers;
using asm.Newtonsoft.Serialization;
using asm.Patterns.Imaging;
using AutoMapper;
using MatchNBuy.API.Filters;
using MatchNBuy.API.ImageBuilders;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using MatchNBuy.Model.ImageBuilders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scrutor;
using Swashbuckle.AspNetCore.Filters;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MatchNBuy.API
{
	public class Startup
	{
		[NotNull]
		private readonly IHostEnvironment _environment;

		[NotNull]
		private readonly IConfiguration _configuration;

		[NotNull]
		private readonly ILogger _logger;

		public Startup([NotNull] IHostEnvironment environment, [NotNull] IConfiguration configuration, [NotNull] ILogger<Startup> logger)
		{
			_environment = environment;
			_configuration = configuration;
			_logger = logger;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices([NotNull] IServiceCollection services)
		{
			string[] allowedClients = _configuration.GetValue("AllowedClients", "*").Split(';', StringSplitOptions.RemoveEmptyEntries);
			services
				// config
				.AddSingleton(_configuration)
				.AddSingleton(_environment)
				// logging
				.AddLogging(config =>
				{
					config
						.AddDebug()
						.AddConsole()
						.AddSerilog();
				})
				.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
				// Swagger
				.AddSwaggerGen(options =>
				{
					options.Setup(_configuration, _environment)
							.AddJwtBearerSecurity();
					//options.OperationFilter<FormFileFilter>();
					options.ExampleFilters();
				})
				.AddSwaggerExamplesFromAssemblyOf<Startup>()
				// Cookies
				.Configure<CookiePolicyOptions>(options =>
				{
					// This lambda determines whether user consent for non-essential cookies is needed for a given request.
					options.CheckConsentNeeded = context => true;
					options.MinimumSameSitePolicy = SameSiteMode.None;
				})
				// FormOptions
				.Configure<FormOptions>(options =>
				{
					options.ValueLengthLimit = int.MaxValue;
					options.MultipartBodyLengthLimit = int.MaxValue;
					options.MemoryBufferThreshold = int.MaxValue;
				})
				// Helpers
				.AddHttpContextAccessor()
				// Image Builders
				.AddSingleton<IUserImageBuilder, UserImageBuilder>()
				// Mapper
				.AddAutoMapper((provider, builder) => builder.AddProfile(new AutoMapperProfiles()),
								new [] { typeof(AutoMapperProfiles).Assembly },
								ServiceLifetime.Singleton)
				// Database
				.AddDbContext<DataContext>(builder =>
				{
					// https://docs.microsoft.com/en-us/ef/core/querying/tracking
					// https://stackoverflow.com/questions/12726878/global-setting-for-asnotracking
					//builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
					IConfigurationSection dataSection = _configuration.GetSection("data");
					bool enableLogging = dataSection.GetValue<bool>("logging");

					if (enableLogging)
					{
						builder.UseLoggerFactory(LogFactoryHelper.ConsoleLoggerFactory)
							.EnableSensitiveDataLogging();
					}

					builder.UseLazyLoadingProxies();
					builder.UseSqlite(dataSection.GetConnectionString("DefaultConnection"), e => e.MigrationsAssembly(typeof(DataContext).Assembly.GetName().Name));
					builder.EnableDetailedErrors(_environment.IsDevelopment());
				}/*, ServiceLifetime.Singleton*/)
				// Add CityRepository for a special case: needs to be a Singleton
				.AddSingleton<ICityRepositoryBase>(provider =>
				{
					// don't dispose the scope because it'll dispose the DataContext
					IServiceScope scope = provider.CreateScope();
					IServiceProvider scopedProvider = scope.ServiceProvider;
					return new CityRepository(scopedProvider.GetRequiredService<DataContext>(),
											_configuration,
											scopedProvider.GetService<ILogger<CityRepository>>());
				})
				// using Scrutor
				.Scan(scan =>
				{
					// Add all repositories
					scan.FromAssemblyOf<DataContext>()
						// Repositories will have scoped life time
						.AddClasses(classes => classes.AssignableTo<IRepositoryBase>())
						.UsingRegistrationStrategy(RegistrationStrategy.Skip)
						.AsImplementedInterfaces()
						.AsSelf()
						.WithScopedLifetime();

					// Can also use this approach: (This will require DataContext to be registered as Singleton which is not recommended)
					//scan.FromAssemblyOf<DataContext>()
					//	// Edit repositories will have scoped life time
					//	.AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
					//	.UsingRegistrationStrategy(RegistrationStrategy.Skip)
					//	.AsImplementedInterfaces()
					//	.AsSelf()
					//	.WithScopedLifetime()
					//	// ReadOnly repositories will have singleton life time
					//	.AddClasses(classes => classes.AssignableTo<IRepositoryBase>())
					//	.UsingRegistrationStrategy(RegistrationStrategy.Skip)
					//	.AsImplementedInterfaces()
					//	.AsSelf()
					//	.WithSingletonLifetime();

					// Add image builders
					scan.FromAssemblyOf<Startup>()
						.AddClasses(classes => classes.AssignableTo<IImageBuilder>())
						.UsingRegistrationStrategy(RegistrationStrategy.Skip)
						.AsImplementedInterfaces()
						.AsSelf()
						.WithSingletonLifetime();
				})
				// Identity
				.AddIdentityCore<User>(options =>
				{
					options.Stores.MaxLengthForKeys = 128;
					options.User.RequireUniqueEmail = true;
				})
				.AddRoles<Role>()
				.AddEntityFrameworkStores<DataContext>()
				.AddUserManager<UserManager<User>>()
				.AddRoleManager<RoleManager<Role>>()
				.AddRoleValidator<RoleValidator<Role>>()
				.AddSignInManager<SignInManager<User>>()
				.AddDefaultTokenProviders()
				.Services
				// IdentityServer
				//.AddIdentityServer()
				//	.AddDeveloperSigningCredential()
				//	.AddInMemoryPersistedGrants()
				//	.AddInMemoryIdentityResources(Config.GetIdentityResources())
				//	.AddInMemoryApiResources(Config.GetApiResources())
				//	.AddInMemoryClients(Config.GetClients())
				//	.AddAspNetIdentity<User>()
				// Jwt Bearer
				.AddJwtBearerAuthentication()
				.AddCookie(options =>
				{
					options.SlidingExpiration = true;
					options.LoginPath = "/users/login";
					options.LogoutPath = "/users/logout";
					options.ExpireTimeSpan = TimeSpan.FromMinutes(_configuration.GetValue("jwt:timeout", 20).NotBelow(5));
				})
				.AddJwtBearerOptions(options =>
				{
					SecurityKey signingKey = SecurityKeyHelper.CreateSymmetricKey(_configuration.GetValue<string>("jwt:signingKey"), 256);
					//SecurityKey decryptionKey = SecurityKeyHelper.CreateSymmetricKey(_configuration.GetValue<string>("jwt:encryptionKey"), 256);
					options.Setup(signingKey, /*decryptionKey, */_configuration, _environment.IsDevelopment());
				})
				.Services
				.AddAuthorization(options =>
				{
					options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
											.RequireAuthenticatedUser()
											.RequireClaim(ClaimTypes.Role, Role.Roles)
											.Build();

					options.AddPolicy(Role.Members, policy =>
					{
						policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
							.RequireAuthenticatedUser()
							.RequireClaim(ClaimTypes.Role, Role.Members)
							.RequireRole(Role.Members);
					});

					options.AddPolicy(Role.Administrators, policy =>
					{
						policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
							.RequireAuthenticatedUser()
							.RequireClaim(ClaimTypes.Role, Role.Administrators)
							.RequireRole(Role.Administrators);
					});
				})
				// MVC
				.AddDefaultCorsPolicy(builder => builder.WithExposedHeaders("Set-Cookie"), allowedClients)
				.AddForwardedHeaders()
				// Filters
				.AddScoped<LogUserActivity>()
				.AddControllers()
				.AddNewtonsoftJson(options =>
				{
					JsonHelper.SetDefaults(options.SerializerSettings, contractResolver: new CamelCasePropertyNamesContractResolver());
					options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

					JsonSerializerSettingsConverters allConverters = EnumHelper<JsonSerializerSettingsConverters>.GetAllFlags() &
																	~(JsonSerializerSettingsConverters.IsoDateTime |
																	JsonSerializerSettingsConverters.JavaScriptDateTime |
																	JsonSerializerSettingsConverters.UnixDateTime);
					options.SerializerSettings.AddConverters(allConverters);
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseDefaultExceptionDelegate(_logger);
			if (!env.IsDevelopment()) app.UseHsts();

			app.UseHttpsRedirection()
				.UseForwardedHeaders()
				.UseCultureHandler()
				.UseSerilogRequestLogging()
				.UseSwagger(config => config.RouteTemplate = _configuration.GetValue<string>("swagger:template"))
				.UseSwaggerUI(config =>
				{
					config.SwaggerEndpoint(_configuration.GetValue<string>("swagger:ui"), _configuration.GetValue("title", _environment.ApplicationName));
					config.AsStartPage();
				})
				.UseDefaultFiles()
				.UseStaticFiles(new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(AssemblyHelper.GetEntryAssembly().GetDirectoryPath())
				})
				.UseCookiePolicy(new CookiePolicyOptions
				{
					MinimumSameSitePolicy = SameSiteMode.None,
					Secure = CookieSecurePolicy.SameAsRequest
				})
				.UseRouting()
				.UseCors()
				.UseAuthentication()
				.UseAuthorization()
				.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();

					// Last route
					endpoints.MapDefaultRoutes();
				});
		}
	}
}
