using System;
using System.Security.Claims;
using asm.Core.Authentication.JwtBearer.Extensions;
using asm.Core.Authentication.JwtBearer.Helpers;
using asm.Core.Web.Extensions;
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
using asm.Newtonsoft.Extensions;
using asm.Newtonsoft.Helpers;
using asm.Core.Swagger.Extensions;
using asm.Data.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using asm.Newtonsoft.Serialization;
using asm.Patterns.Images;
using AutoMapper;
using MatchNBuy.API.ImageBuilders;
using MatchNBuy.Data.ImageBuilders;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
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
		public void ConfigureServices(IServiceCollection services)
		{
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

				.AddSingleton<IUserImageBuilder, UserImageBuilder>()
				// Mapper
				.AddAutoMapper(options => options.AddProfile(new AutoMapperProfiles()),
								new []{typeof(AutoMapperProfiles).Assembly}, 
								ServiceLifetime.Singleton)
				// Database
				.AddDbContext<DataContext>((provider, builder) =>
				{
					builder.UseLazyLoadingProxies();
					// https://docs.microsoft.com/en-us/ef/core/querying/tracking
					// https://stackoverflow.com/questions/12726878/global-setting-for-asnotracking
					//builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
					builder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"), e => e.MigrationsAssembly(typeof(DataContext).Assembly.GetName().Name));
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
				.AddCookie(options => options.SlidingExpiration = true)
				.AddJwtBearerOptions(options =>
				{
					SecurityKey signingKey = SecurityKeyHelper.CreateSymmetricKey(_configuration.GetValue<string>("jwt:signingKey"), 256);
					SecurityKey decryptionKey = SecurityKeyHelper.CreateSymmetricKey(_configuration.GetValue<string>("jwt:encryptionKey"), 256);
					options.Setup(signingKey, decryptionKey, _configuration, _environment.IsDevelopment());
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
				.AddDefaultCors()
				.AddForwardedHeaders()
				.AddControllers()
				.AddNewtonsoftJson(options =>
				{
					JsonHelper.SetDefaults(options.SerializerSettings, contractResolver: new CamelCasePropertyNamesContractResolver());

					/*
					 * add all the converters except date formatters.
					 * Why? because the formatted date sometimes lack the time
					 * zone anyway, which is weird ;(. Not sure why the serializer
					 * sometimes produces dates with timezone and sometimes not.
					 * Is this another bug?
					 *
					 * Another reason is the date actually will be a string on the other
					 * end in spite of the damn type specification! Which means we'll have
					 * to deal with both JS date and string even if the TypeScript has date
					 * type specified. Apparently, the type is lost when converted to JavaScript
					 * code. All in all, JS date time zone's is not reliable and its compatibility
					 * is absolute garbage!! So forget it.
					 *
					 * The better option is to fix the date format string.
					 * options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
					 * is important otherwise, the serializer will suddenly use Microsoft date format Date(number)
					 * because some psycho decided for some weird unwelcome reason to use the strange MS date format.
					 * Take your wildest guesses!
					 */
					options.SerializerSettings.DateFormatString = DateTimeHelper.LONG_DATE_TIME_FORMAT;
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
				.UseStaticFiles()
				.UseCookiePolicy()
				.UseRouting()
				.UseCors()
				.UseAuthentication()
				.UseAuthorization()
				.UseStaticFiles(new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(AssemblyHelper.GetEntryAssembly().GetDirectoryPath())
				})
				.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();

					// Last route
					endpoints.MapDefaultRoutes();
				});
		}
	}
}
