using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using MatchNBuy.Model;
using JetBrains.Annotations;
using MatchNBuy.Model.ImageBuilders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MatchNBuy.Data.Repositories
{
	public class UserRepository : Repository<DataContext, User>, IUserRepository
	{
		private static readonly Lazy<PropertyInfo[]> __keyProperties = new Lazy<PropertyInfo[]>(() => new[] { typeof(User).GetProperty(nameof(User.Id)) }, LazyThreadSafetyMode.PublicationOnly);

		/// <inheritdoc />
		public UserRepository([NotNull] DataContext context, [NotNull] SignInManager<User> signInManager, [NotNull] IPhotoRepository photosRepository, [NotNull] IMessageRepository messageRepository, [NotNull] IUserImageBuilder userImageBuilder, [NotNull] IConfiguration configuration, ILogger<UserRepository> logger)
			: base(context, configuration, logger)
		{
			SignInManager = signInManager;
			UserManager = SignInManager.UserManager;
			Photos = photosRepository;
			Messages = messageRepository;
			ImageBuilder = userImageBuilder;
		}

		/// <inheritdoc />
		protected override PropertyInfo[] KeyProperties => __keyProperties.Value;

		public IPhotoRepository Photos { get; }

		public IMessageRepository Messages { get; }

		public IUserImageBuilder ImageBuilder { get; }

		[NotNull]
		protected UserManager<User> UserManager { get; }

		[NotNull]
		protected SignInManager<User> SignInManager { get; }

		/// <inheritdoc />
		protected override User GetInternal([NotNull] params object[] keys)
		{
			return UserManager.FindByIdAsync((string)keys[0]).GetAwaiter().GetResult();
		}

		/// <inheritdoc />
		protected override ValueTask<User> GetAsyncInternal([NotNull] object[] keys, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<User>(UserManager.FindByIdAsync((string)keys[0]));
		}

		public User Get(ClaimsPrincipal principal)
		{
			ThrowIfDisposed();
			return UserManager.GetUserAsync(principal).GetAwaiter().GetResult();
		}

		public ValueTask<User> GetAsync(ClaimsPrincipal principal, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<User>(UserManager.GetUserAsync(principal));
		}

		/// <inheritdoc />
		[NotNull]
		protected override User AddInternal([NotNull] User entity) { return Add(entity, null); }

		[NotNull]
		public User Add(User entity, string password)
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
			entity.Created = DateTime.UtcNow;
			entity.Modified = DateTime.UtcNow;
			entity.LastActive = DateTime.UtcNow;
			entity.LockoutEnabled = true;

			IdentityResult result = (string.IsNullOrEmpty(password)
										? UserManager.CreateAsync(entity)
										: UserManager.CreateAsync(entity, password)).GetAwaiter()
																					.GetResult();
			if (!result.Succeeded) throw new Exception(result.Errors.CollectMessages($"Unable to create user '{entity.UserName}'."));
			result = UserManager.AddToRoleAsync(entity, Role.Members).GetAwaiter().GetResult();
			if (!result.Succeeded) throw new Exception(result.Errors.CollectMessages($"User '{entity.UserName}' is created but with some errors."));
			return entity;
		}

		/// <inheritdoc />
		protected override ValueTask<User> AddAsyncInternal([NotNull] User entity, CancellationToken token = default(CancellationToken))
		{
			return AddAsync(entity, null, token);
		}

		[ItemNotNull]
		public async ValueTask<User> AddAsync(User entity, string password, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
			entity.Created = DateTime.UtcNow;
			entity.Modified = DateTime.UtcNow;
			entity.LastActive = DateTime.UtcNow;
			entity.LockoutEnabled = true;

			IdentityResult result = string.IsNullOrEmpty(password)
										? await UserManager.CreateAsync(entity)
										: await UserManager.CreateAsync(entity, password);
			token.ThrowIfCancellationRequested();
			if (!result.Succeeded) throw new Exception(result.Errors.CollectMessages($"Unable to create user '{entity.UserName}'."));
			result = await UserManager.AddToRoleAsync(entity, Role.Members);
			token.ThrowIfCancellationRequested();
			if (!result.Succeeded) throw new Exception(result.Errors.CollectMessages($"User '{entity.UserName}' is created but with some errors."));
			return entity;
		}

		/// <inheritdoc />
		[NotNull]
		protected override User UpdateInternal([NotNull] User entity) { return UpdateAsyncInternal(entity).GetAwaiter().GetResult(); }

		/// <inheritdoc />
		[ItemNotNull]
		protected override async ValueTask<User> UpdateAsyncInternal([NotNull] User entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			IdentityResult result = await UserManager.UpdateAsync(entity);
			token.ThrowIfCancellationRequested();
			if (result.Succeeded) return entity;
			throw new Exception(result.Errors.CollectMessages($"Unable to update user '{entity.UserName}'."));
		}

		/// <inheritdoc />
		[NotNull]
		protected override User DeleteInternal([NotNull] User entity) { return DeleteAsyncInternal(entity).GetAwaiter().GetResult(); }

		/// <inheritdoc />
		[ItemNotNull]
		protected override async ValueTask<User> DeleteAsyncInternal([NotNull] User entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			bool changed = false;

			if (entity.UserRoles?.Count > 0)
			{
				Context.UserRoles.RemoveRange(entity.UserRoles);
				changed = true;
			}

			if (entity.UserInterests?.Count > 0)
			{
				Context.UserInterests.RemoveRange(entity.UserInterests);
				changed = true;
			}

			if (entity.Photos?.Count > 0)
			{
				Context.Photos.RemoveRange(entity.Photos);
				changed = true;
			}

			if (entity.Likers?.Count > 0)
			{
				Context.Likes.RemoveRange(entity.Likers);
				changed = true;
			}

			if (entity.Likees?.Count > 0)
			{
				Context.Likes.RemoveRange(entity.Likees);
				changed = true;
			}

			if (entity.MessagesSent?.Count > 0)
			{
				Context.Messages.RemoveRange(entity.MessagesSent);
				changed = true;
			}

			if (entity.MessagesReceived?.Count > 0)
			{
				Context.Messages.RemoveRange(entity.MessagesReceived);
				changed = true;
			}

			if (changed) await Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();

			IList<Claim> claims = await GetClaimsAsync(entity, token);
			token.ThrowIfCancellationRequested();

			IdentityResult result;

			if (claims != null && claims.Count > 0)
			{
				result = await UserManager.RemoveClaimsAsync(entity, claims);
				token.ThrowIfCancellationRequested();
				if (!result.Succeeded) throw new Exception(result.Errors.CollectMessages($"Unable to delete user '{entity.UserName}'."));
			}

			result = await UserManager.DeleteAsync(entity);
			token.ThrowIfCancellationRequested();
			if (result.Succeeded) return entity;
			throw new Exception(result.Errors.CollectMessages($"Unable to delete user '{entity.UserName}'."));
		}

		public ValueTask<bool> CheckPasswordAsync(User entity, string password, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.CheckPasswordAsync(entity, password));
		}

		public string NormalizeName(string name) { return UserManager.NormalizeName(name); }

		public string NormalizeEmail(string email) { return UserManager.NormalizeEmail(email); }

		public string GetUserId(ClaimsPrincipal principal) { return UserManager.GetUserId(principal); }

		public string GetUserName(ClaimsPrincipal principal) { return UserManager.GetUserName(principal); }

		public User FindByName(string userName)
		{
			ThrowIfDisposed();
			return UserManager.FindByNameAsync(userName).GetAwaiter().GetResult();
		}

		public ValueTask<User> FindByNameAsync(string userName, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<User>(UserManager.FindByNameAsync(userName));
		}

		public User FindByEmail(string email)
		{
			ThrowIfDisposed();
			return UserManager.FindByEmailAsync(email).GetAwaiter().GetResult();
		}

		public ValueTask<User> FindByEmailAsync(string email, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<User>(UserManager.FindByEmailAsync(email));
		}

		public ValueTask<string> GenerateEmailConfirmationTokenAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<string>(UserManager.GenerateEmailConfirmationTokenAsync(entity));
		}

		public ValueTask<IdentityResult> ConfirmEmailAsync(User entity, string confirmationToken, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.ConfirmEmailAsync(entity, confirmationToken));
		}

		public ValueTask<bool> IsEmailConfirmedAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.IsEmailConfirmedAsync(entity));
		}

		public ValueTask<bool> HasPasswordAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.HasPasswordAsync(entity));
		}

		public ValueTask<IdentityResult> AddPasswordAsync(User entity, string password, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.AddPasswordAsync(entity, password));
		}

		public ValueTask<IdentityResult> ChangePasswordAsync(User entity, string currentPassword, string newPassword, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.ChangePasswordAsync(entity, currentPassword, newPassword));
		}

		public ValueTask<IdentityResult> RemovePasswordAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.RemovePasswordAsync(entity));
		}

		public ValueTask<string> GeneratePasswordResetTokenAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<string>(UserManager.GeneratePasswordResetTokenAsync(entity));
		}

		public ValueTask<IdentityResult> ResetPasswordAsync(User entity, string confirmationToken, string newPassword, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.ResetPasswordAsync(entity, confirmationToken, newPassword));
		}

		public ValueTask<IList<Claim>> GetClaimsAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IList<Claim>>(UserManager.GetClaimsAsync(entity));
		}

		public ValueTask<IdentityResult> AddClaimAsync(User entity, Claim claim, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.AddClaimAsync(entity, claim));
		}

		public ValueTask<IdentityResult> AddClaimsAsync(User entity, IEnumerable<Claim> claims, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.AddClaimsAsync(entity, claims));
		}

		public ValueTask<IdentityResult> ReplaceClaimAsync(User entity, Claim claim, Claim newClaim, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.ReplaceClaimAsync(entity, claim, newClaim));
		}

		public ValueTask<IdentityResult> RemoveClaimAsync(User entity, Claim claim, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.RemoveClaimAsync(entity, claim));
		}

		public ValueTask<IdentityResult> RemoveClaimsAsync(User entity, IEnumerable<Claim> claims, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.RemoveClaimsAsync(entity, claims));
		}

		public ValueTask<IList<string>> GetRolesAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IList<string>>(UserManager.GetRolesAsync(entity));
		}

		public ValueTask<bool> IsInRoleAsync(User entity, string role, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.IsInRoleAsync(entity, role));
		}

		public ValueTask<IdentityResult> AddToRoleAsync(User entity, string role, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.AddToRoleAsync(entity, role));
		}

		public ValueTask<IdentityResult> AddToRolesAsync(User entity, IEnumerable<string> roles, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.AddToRolesAsync(entity, roles));
		}

		public ValueTask<IdentityResult> RemoveFromRoleAsync(User entity, string role, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.RemoveFromRoleAsync(entity, role));
		}

		public ValueTask<IdentityResult> RemoveFromRolesAsync(User entity, IEnumerable<string> roles, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.RemoveFromRolesAsync(entity, roles));
		}

		public ValueTask<string> GetPhoneNumberAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<string>(UserManager.GetPhoneNumberAsync(entity));
		}

		public ValueTask<IdentityResult> SetPhoneNumberAsync(User entity, string phoneNumber, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.SetPhoneNumberAsync(entity, phoneNumber));
		}

		public ValueTask<IdentityResult> ChangePhoneNumberAsync(User entity, string phoneNumber, string confirmationToken, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.ChangePhoneNumberAsync(entity, phoneNumber, confirmationToken));
		}

		public ValueTask<bool> IsPhoneNumberConfirmedAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.IsPhoneNumberConfirmedAsync(entity));
		}

		public ValueTask<string> GenerateChangePhoneNumberTokenAsync(User entity, string phoneNumber, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<string>(UserManager.GenerateChangePhoneNumberTokenAsync(entity, phoneNumber));
		}

		public ValueTask<bool> VerifyChangePhoneNumberTokenAsync(User entity, string confirmationToken, string phoneNumber, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.VerifyChangePhoneNumberTokenAsync(entity, confirmationToken, phoneNumber));
		}

		public ValueTask<string> GenerateUserTokenAsync(User entity, string tokenProvider, string purpose, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<string>(UserManager.GenerateUserTokenAsync(entity, tokenProvider, purpose));
		}

		public ValueTask<bool> IsLockedOutAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(UserManager.IsLockedOutAsync(entity));
		}

		public ValueTask<DateTimeOffset?> GetLockoutEndDateAsync(User entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<DateTimeOffset?>(UserManager.GetLockoutEndDateAsync(entity));
		}

		public ValueTask<IdentityResult> SetLockoutEndDateAsync(User entity, DateTimeOffset? lockoutEnd, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IdentityResult>(UserManager.SetLockoutEndDateAsync(entity, lockoutEnd));
		}

		public ValueTask<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IList<User>>(UserManager.GetUsersForClaimAsync(claim));
		}

		public ValueTask<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<IList<User>>(UserManager.GetUsersInRoleAsync(roleName));
		}

		public ValueTask<ClaimsPrincipal> CreateUserPrincipalAsync(User user, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<ClaimsPrincipal>(SignInManager.CreateUserPrincipalAsync(user));
		}

		public async ValueTask<TokenSignInResult> SignInAsync(string userName, string password, bool lockoutOnFailure, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			User user = await FindByNameAsync(userName, token);
			return user == null
						? TokenSignInResult.NotAllowed
						: await SignInAsync(user, password, lockoutOnFailure, token);
		}

		public async ValueTask<TokenSignInResult> SignInAsync(User user, string password, bool lockoutOnFailure, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			if (string.IsNullOrEmpty(password))
			{
				await SignInManager.SignInAsync(user, true);
				token.ThrowIfCancellationRequested();
			}
			else
			{
				SignInResult result = await SignInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
				token.ThrowIfCancellationRequested();
				if (result.RequiresTwoFactor) return TokenSignInResult.TwoFactorRequired;
				if (result.IsLockedOut) return TokenSignInResult.LockedOut;
				if (!result.Succeeded) return TokenSignInResult.NotAllowed;
			}

			RefreshToken refreshToken = GenerateRefreshToken(user);
			user.LastActive = DateTime.UtcNow;
			Context.Update(user);
			await Context.SaveChangesAsync(token);
			string accessToken = await GenerateToken(user, token);
			return TokenSignInResult.SuccessFrom(user, accessToken, refreshToken.Value);
		}

		public async ValueTask<TokenSignInResult> RefreshTokenAsync(string refreshToken, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(refreshToken)) return TokenSignInResult.NotAllowed;

			User user = await Context.Users.Include(e => e.RefreshTokens)
									.FirstOrDefaultAsync(e => e.RefreshTokens.Any(r => r.Value == refreshToken), token);
			return user == null
						? TokenSignInResult.NotAllowed
						: await RefreshTokenAsync(user, token);
		}

		public async ValueTask<TokenSignInResult> RefreshTokenAsync(RefreshToken refreshToken, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (!refreshToken.IsActive) return TokenSignInResult.NotAllowed;

			User user = refreshToken.User;

			if (user == null)
			{
				if (string.IsNullOrEmpty(refreshToken.UserId)) return TokenSignInResult.NotAllowed;
				user = await DbSet.FindAsync(refreshToken.UserId);
				token.ThrowIfCancellationRequested();
				if (user == null) return TokenSignInResult.NotAllowed;
			}

			return await RefreshTokenAsync(user, token);
		}

		public async ValueTask<TokenSignInResult> RefreshTokenAsync(User user, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			await SignInManager.RefreshSignInAsync(user);

			RefreshToken newRefreshToken = GenerateRefreshToken(user, true);
			user.LastActive = DateTime.UtcNow;
			Context.Update(user);
			await Context.SaveChangesAsync(token);
			string accessToken = await GenerateToken(user, token);
			return TokenSignInResult.SuccessFrom(user, accessToken, newRefreshToken.Value);
		}

		public async Task LogoutAsync(string userId, bool logoutFromAllDevices = false, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

			User user = await DbSet.FindAsync(userId);
			if (user == null) return;
			await LogoutAsync(user, logoutFromAllDevices, token);
		}

		public async Task LogoutAsync(User user, bool logoutFromAllDevices = false, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			HttpRequest request = SignInManager.Context?.Request;
			string source = request.GetIPAddress()?.ToLowerInvariant();
			IList<RefreshToken> tokens = user.RefreshTokens
												.Where(e => e.Revoked == null && (!logoutFromAllDevices || e.CreatedBy == source))
												.ToList();
			if (tokens.Count == 0) return;

			foreach (RefreshToken refreshToken in tokens)
			{
				refreshToken.Revoked = DateTime.UtcNow;
				refreshToken.RevokedBy = source;
				Context.Update(refreshToken);
			}

			await Context.SaveChangesAsync(token);
		}

		public async Task LogoutByTokenAsync(string refreshToken, bool logoutFromAllDevices = false, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(refreshToken)) return;

			User user = await Context.Users
									.Include(e => e.RefreshTokens)
									.FirstOrDefaultAsync(e => e.RefreshTokens.Any(t => t.Value == refreshToken), token);
			if (user == null) return;
			await LogoutAsync(user, logoutFromAllDevices, token);
		}

		public Task LogoutByTokenAsync(RefreshToken refreshToken, bool logoutFromAllDevices = false, CancellationToken token = default(CancellationToken))
		{
			return LogoutAsync(refreshToken.User, logoutFromAllDevices, token);
		}

		public bool IsSignedIn(ClaimsPrincipal principal)
		{
			ThrowIfDisposed();
			return SignInManager.IsSignedIn(principal);
		}

		public ValueTask<bool> CanSignInAsync(User user, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(SignInManager.CanSignInAsync(user));
		}

		public ValueTask<User> ValidateSecurityStampAsync(ClaimsPrincipal principal, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<User>(SignInManager.ValidateSecurityStampAsync(principal));
		}

		public ValueTask<bool> ValidateSecurityStampAsync(User user, string securityStamp, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return new ValueTask<bool>(SignInManager.ValidateSecurityStampAsync(user, securityStamp));
		}

		/// <inheritdoc />
		public int Like(string userId, string recipientId)
		{
			ThrowIfDisposed();

			if (Context.Likes.FirstOrDefault(e => e.LikerId == userId && e.LikeeId == recipientId) == null)
			{
				Context.Likes.Add(new Like
				{
					LikerId = userId,
					LikeeId = recipientId
				});

				Context.SaveChanges();
			}

			return Likes(recipientId);
		}

		/// <inheritdoc />
		public async ValueTask<int> LikeAsync(string userId, string recipientId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			if (await Context.Likes.FirstOrDefaultAsync(e => e.LikerId == userId && e.LikeeId == recipientId, token) == null)
			{
				await Context.Likes.AddAsync(new Like
				{
					LikerId = userId,
					LikeeId = recipientId
				}, token);

				token.ThrowIfCancellationRequested();
				await Context.SaveChangesAsync(token);
			}

			return await LikesAsync(recipientId, token);
		}

		/// <inheritdoc />
		public int Dislike(string userId, string recipientId)
		{
			ThrowIfDisposed();

			Like like = Context.Likes.FirstOrDefault(e => e.LikerId == userId && e.LikeeId == recipientId);

			if (like != null)
			{
				Context.Likes.Remove(like);
				Context.SaveChanges();
			}

			return Likes(recipientId);
		}

		/// <inheritdoc />
		public async ValueTask<int> DislikeAsync(string userId, string recipientId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			Like like = await Context.Likes.FirstOrDefaultAsync(e => e.LikerId == userId && e.LikeeId == recipientId, token);
			token.ThrowIfCancellationRequested();

			if (like != null)
			{
				Context.Likes.Remove(like);
				await Context.SaveChangesAsync(token);
			}

			return await LikesAsync(recipientId, token);
		}

		/// <inheritdoc />
		public int Likes(string userId)
		{
			ThrowIfDisposed();
			return Context.Likes.Count(e => e.LikeeId == userId);
		}

		/// <inheritdoc />
		public async ValueTask<int> LikesAsync(string userId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return await Context.Likes.CountAsync(e => e.LikeeId == userId, token);
		}

		/// <inheritdoc />
		public int Likees(string userId)
		{
			ThrowIfDisposed();
			return Context.Likes.Count(e => e.LikerId == userId);
		}

		/// <inheritdoc />
		public async ValueTask<int> LikeesAsync(string userId, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return await Context.Likes.CountAsync(e => e.LikerId == userId, token);
		}

		/// <inheritdoc />
		public bool IsLikee(string userId, string id)
		{
			ThrowIfDisposed();
			return Context.Likes
						.FirstOrDefault(e => e.LikerId == userId && id == e.LikeeId) != null;
		}

		/// <inheritdoc />
		public async ValueTask<bool> IsLikeeAsync(string userId, string id, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return await Context.Likes
								.FirstOrDefaultAsync(e => e.LikerId == userId && id == e.LikeeId, token) != null;
		}

		/// <inheritdoc />
		public ISet<string> LikeesFromList(string userId, params string[] idList) { return LikeesFromList(userId, (IEnumerable<string>)idList); }
		/// <inheritdoc />
		public ISet<string> LikeesFromList(string userId, IEnumerable<string> idList)
		{
			ThrowIfDisposed();
			IList<string> ids = idList as IList<string> ?? idList.ToList();
			return Context.Likes
						.Where(e => e.LikerId == userId && ids.Contains(e.LikeeId))
						.Select(e => e.LikeeId)
						.ToHashSet(StringComparer.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		public async ValueTask<ISet<string>> LikeesFromListAsync(string userId, IEnumerable<string> idList, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			IList<string> ids = idList as IList<string> ?? idList.ToList();
			IAsyncEnumerable<string> enumerable = Context.Likes
														.Where(e => e.LikerId == userId && ids.Contains(e.LikeeId))
														.Select(e => e.LikeeId)
														.AsAsyncEnumerable();
			HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			await foreach (string id in enumerable.WithCancellation(token))
			{
				set.Add(id);
			}

			return set;
		}

		public int GetTokenExpirationTime()
		{
			const int TOKEN_MIN = 1;
			const int TOKEN_MAX = 1440;
			const int TOKEN_DEF = 20;

			return Configuration.GetValue<int>("jwt:timeout")
								.Within(0, TOKEN_MAX)
								.IfLessThan(TOKEN_MIN, TOKEN_DEF);
		}

		public int GetRefreshTokenExpirationTime()
		{
			const int TOKEN_MIN = 1;
			const int TOKEN_MAX = 1440;
			const int TOKEN_DEF = 720;

			return Configuration.GetValue<int>("jwt:refreshInterval")
								.Within(0, TOKEN_MAX)
								.IfLessThan(TOKEN_MIN, TOKEN_DEF);
		}

		private async Task<string> GenerateToken([NotNull] User user, CancellationToken token = default(CancellationToken))
		{
			const string AUDIENCE_DEF = "api://default";

			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			ClaimsPrincipal principal = await SignInManager.CreateUserPrincipalAsync(user);
			IList<string> roles = await UserManager.GetRolesAsync(user);
			token.ThrowIfCancellationRequested();

			ClaimsIdentity subject = new ClaimsIdentity(principal.Identity, null, JwtBearerDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
			subject.AddClaim(new Claim(ClaimTypes.GivenName, user.KnownAs ?? $"{user.FirstName} {user.LastName}".Trim()));
			subject.AddClaim(new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty));
			subject.AddClaim(new Claim(ClaimTypes.Email, user.Email));
			subject.AddClaim(new Claim(ClaimTypes.Gender, user.Gender.ToString()));
			subject.AddClaim(new Claim(ClaimTypes.DateOfBirth, user.DateOfBirth.ToString(User.DATE_FORMAT)));
			subject.AddClaim(new Claim(ClaimTypes.Uri, user.PhotoUrl ?? string.Empty));
			subject.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

			foreach (string role in roles)
				subject.AddClaim(new Claim(ClaimTypes.Role, role));

			HttpRequest request = SignInManager.Context?.Request;
			string host = request?.Host.Host;
			string audience = Configuration.GetValue("jwt:audience", AUDIENCE_DEF);
			string signingKey = Configuration.GetValue<string>("jwt:signingKey");
			string encryptionKey = Configuration.GetValue<string>("jwt:encryptionKey");

			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
			{
				Issuer = host,
				Audience = audience,
				Subject = subject,
				IssuedAt = DateTime.UtcNow,
				Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationTime()),
			};

			if (!string.IsNullOrEmpty(signingKey))
				tokenDescriptor.SigningCredentials = new SigningCredentials(SecurityKeyHelper.CreateSymmetricKey(signingKey, 256), SecurityAlgorithms.HmacSha256Signature);

			// It is not recommended to use this if JS libraries will handle tokens unless they can and will do decryption.
			if (!string.IsNullOrEmpty(encryptionKey))
				tokenDescriptor.EncryptingCredentials = new EncryptingCredentials(SecurityKeyHelper.CreateSymmetricKey(encryptionKey, 256), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes128CbcHmacSha256);

			JwtSecurityToken securityToken = SecurityTokenHelper.CreateToken(tokenDescriptor);
			return SecurityTokenHelper.Value(securityToken);
		}

		[NotNull]
		private RefreshToken GenerateRefreshToken([NotNull] User user, bool forceNew = false)
		{
			HttpRequest request = SignInManager.Context?.Request;
			string source = request.GetIPAddress()?.ToLowerInvariant();
			IList<RefreshToken> openTokens = user.RefreshTokens
										.Where(e => e.CreatedBy == source && e.Revoked == null)
										.OrderByDescending(e => e.Expires)
										.ToList();
			int offset = 0;
			RefreshToken refreshToken = null;

			if (!forceNew && openTokens.Count > 0)
			{
				refreshToken = openTokens[0];

				if ((refreshToken.Expires - DateTime.UtcNow).TotalSeconds < 5.0d) 
					refreshToken = null;
				else
					offset++;
			}

			if (refreshToken == null)
			{
				byte[] buffer = new byte[64];
				RNGRandomHelper.Default.GetNonZeroBytes(buffer);
				refreshToken = new RefreshToken
				{
					Value = Convert.ToBase64String(buffer),
					UserId = user.Id,
					Expires = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpirationTime()),
					Created = DateTime.UtcNow,
					CreatedBy = source
				};

				user.RefreshTokens.Add(refreshToken);
				offset++;
			}

			for (int i = offset; i < openTokens.Count; i++)
			{
				RefreshToken oldRefreshToken = openTokens[i];
				oldRefreshToken.Revoked = DateTime.UtcNow;
				oldRefreshToken.RevokedBy = refreshToken.CreatedBy;
				oldRefreshToken.ReplacedBy = refreshToken.Value;
			}

			if (offset > 0) Context.Update(user);
			return refreshToken;
		}
	}
}