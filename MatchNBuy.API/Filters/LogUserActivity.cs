using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MatchNBuy.API.Filters
{
	public class LogUserActivity : IAsyncActionFilter
	{
		/// <inheritdoc />
		public async Task OnActionExecutionAsync(ActionExecutingContext context, [NotNull] ActionExecutionDelegate next)
		{
			ActionExecutedContext resultContext = await next();
			string userId = resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			IUserRepository userRepository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
			User user = await userRepository.GetAsync(userId);
			user.LastActive = DateTime.Now;
			await userRepository.Context.SaveChangesAsync();
		}
	}
}
