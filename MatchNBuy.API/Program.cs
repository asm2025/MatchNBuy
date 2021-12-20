using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using essentialMix.Extensions;
using essentialMix.Helpers;
using AutoMapper;
using essentialMix.Core.Web.Helpers;
using JetBrains.Annotations;
using MatchNBuy.Data;
using MatchNBuy.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MatchNBuy.API
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			Directory.SetCurrentDirectory(AssemblyHelper.GetEntryAssembly().GetDirectoryPath());
			
			// Configuration
			IConfiguration configuration = IConfigurationBuilderHelper.CreateConfiguration()
																	.AddConfigurationFiles(EnvironmentHelper.GetEnvironmentName())
																	.AddEnvironmentVariables()
																	.AddUserSecrets()
																	.AddArguments(args)
																	.Build();

			// Logging
			LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
			if (configuration.GetValue<bool>("LoggingEnabled")) loggerConfiguration.ReadFrom.Configuration(configuration);
			Log.Logger = loggerConfiguration.CreateLogger();
			
			IWebHost host = CreateHostBuilder(args).Build();
			ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();
			IServiceScope scope = null;

			try
			{
				scope = host.Services.CreateScope();
				IServiceProvider services = scope.ServiceProvider;
				logger.LogInformation($"{configuration.GetValue<string>("title")} is starting...");
				logger.LogInformation("Checking database migrations...");
				
				DataContext dbContext = services.GetRequiredService<DataContext>();
				
				if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
				{
					await dbContext.Database.MigrateAsync();
					UserManager<User> userManager = services.GetRequiredService<UserManager<User>>();
					RoleManager<Role> roleManager = services.GetRequiredService<RoleManager<Role>>();
					IMapper mapper = services.GetRequiredService<IMapper>();
					IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();
					ILogger seedDataLogger = host.Services.GetRequiredService<ILogger<DataContext>>();
					await dbContext.SeedData(userManager, roleManager, "#A1s9m73!`", mapper, configuration, environment, seedDataLogger);
				}

				await host.RunAsync();
				return 0;
			}
			catch (Exception e)
			{
				logger.LogError(e, e.Message);
				return 1;
			}
			finally
			{
				ObjectHelper.Dispose(ref scope);
				Log.CloseAndFlush();
			}
		}

		[NotNull]
		public static IWebHostBuilder CreateHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
						.UseSerilog()
						.UseContentRoot(AssemblyHelper.GetEntryAssembly().GetDirectoryPath())
						.ConfigureAppConfiguration((context, configurationBuilder) =>
						{
							configurationBuilder.Setup(context.HostingEnvironment)
												.AddConfigurationFiles(context.HostingEnvironment)
												.AddEnvironmentVariables()
												.AddUserSecrets();
							if (args is { Length: > 0 }) configurationBuilder.AddArguments(args);
						})
						.UseStartup<Startup>();
		}
	}
}
