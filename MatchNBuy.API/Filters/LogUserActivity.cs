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
			ClaimsPrincipal principal = resultContext.HttpContext.User;
			if (principal.Identity == null || !principal.Identity.IsAuthenticated) return;

			string userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return;

			IUserRepository userRepository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
			if (userRepository == null) return;

			User user = await userRepository.GetAsync(userId);
			if (user == null) return;

			user.LastActive = DateTime.UtcNow;
			userRepository.Context.Update(user);
			await userRepository.Context.SaveChangesAsync();
		}
	}
}
