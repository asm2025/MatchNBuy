using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Data.Entity.Patterns.Repository;
using MatchNBuy.Model;
using JetBrains.Annotations;
using MatchNBuy.Data.ImageBuilders;
using Microsoft.AspNetCore.Identity;

namespace MatchNBuy.Data.Repositories
{
	public interface IUserRepository : IRepository<DataContext, User>
	{
		[NotNull]
		public IPhotoRepository Photos { get; }
		
		[NotNull]
		public IMessageRepository Messages { get; }
		
		[NotNull]
		public IUserImageBuilder ImageBuilder { get; }
		
		string NormalizeName([NotNull] string name);
		string NormalizeEmail([NotNull] string email);
		string GetUserId([NotNull] ClaimsPrincipal principal);
		string GetUserName([NotNull] ClaimsPrincipal principal);
		User Get([NotNull] ClaimsPrincipal principal);
		ValueTask<User> GetAsync([NotNull] ClaimsPrincipal principal, CancellationToken token = default(CancellationToken));
		User FindByName([NotNull] string userName);
		ValueTask<User> FindByNameAsync([NotNull] string userName, CancellationToken token = default(CancellationToken));
		User FindByEmail([NotNull] string email);
		ValueTask<User> FindByEmailAsync([NotNull] string email, CancellationToken token = default(CancellationToken));
		ValueTask<string> GenerateEmailConfirmationTokenAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> ConfirmEmailAsync([NotNull] User entity, [NotNull] string confirmationToken, CancellationToken token = default(CancellationToken));
		ValueTask<bool> IsEmailConfirmedAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		User Add([NotNull] User entity, string password);
		ValueTask<User> AddAsync([NotNull] User entity, string password, CancellationToken token = default(CancellationToken));
		ValueTask<bool> CheckPasswordAsync([NotNull] User entity, [NotNull] string password, CancellationToken token = default(CancellationToken));
		ValueTask<bool> HasPasswordAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> AddPasswordAsync([NotNull] User entity, [NotNull] string password, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> ChangePasswordAsync([NotNull] User entity, string currentPassword, string newPassword, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> RemovePasswordAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<string> GeneratePasswordResetTokenAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> ResetPasswordAsync([NotNull] User entity, [NotNull] string confirmationToken, string newPassword, CancellationToken token = default(CancellationToken));
		ValueTask<IList<Claim>> GetClaimsAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> AddClaimAsync([NotNull] User entity, [NotNull] Claim claim, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> AddClaimsAsync([NotNull] User entity, [NotNull] IEnumerable<Claim> claims, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> ReplaceClaimAsync([NotNull] User entity, [NotNull] Claim claim, [NotNull] Claim newClaim, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> RemoveClaimAsync([NotNull] User entity, [NotNull] Claim claim, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> RemoveClaimsAsync([NotNull] User entity, [NotNull] IEnumerable<Claim> claims, CancellationToken token = default(CancellationToken));
		ValueTask<IList<string>> GetRolesAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<bool> IsInRoleAsync([NotNull] User entity, [NotNull] string role, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> AddToRoleAsync([NotNull] User entity, [NotNull] string role, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> AddToRolesAsync([NotNull] User entity, [NotNull] IEnumerable<string> roles, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> RemoveFromRoleAsync([NotNull] User entity, [NotNull] string role, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> RemoveFromRolesAsync([NotNull] User entity, [NotNull] IEnumerable<string> roles, CancellationToken token = default(CancellationToken));
		ValueTask<string> GetPhoneNumberAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> SetPhoneNumberAsync([NotNull] User entity, string phoneNumber, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> ChangePhoneNumberAsync([NotNull] User entity, string phoneNumber, [NotNull] string confirmationToken, CancellationToken token = default(CancellationToken));
		ValueTask<bool> IsPhoneNumberConfirmedAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<string> GenerateChangePhoneNumberTokenAsync([NotNull] User entity, string phoneNumber, CancellationToken token = default(CancellationToken));
		ValueTask<bool> VerifyChangePhoneNumberTokenAsync([NotNull] User entity, [NotNull] string confirmationToken, string phoneNumber, CancellationToken token = default(CancellationToken));
		ValueTask<string> GenerateUserTokenAsync([NotNull] User entity, string tokenProvider, string purpose, CancellationToken token = default(CancellationToken));
		ValueTask<bool> IsLockedOutAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<DateTimeOffset?> GetLockoutEndDateAsync([NotNull] User entity, CancellationToken token = default(CancellationToken));
		ValueTask<IdentityResult> SetLockoutEndDateAsync([NotNull] User entity, DateTimeOffset? lockoutEnd, CancellationToken token = default(CancellationToken));
		ValueTask<IList<User>> GetUsersForClaimAsync([NotNull] Claim claim, CancellationToken token = default(CancellationToken));
		ValueTask<IList<User>> GetUsersInRoleAsync([NotNull] string roleName, CancellationToken token = default(CancellationToken));
		ValueTask<ClaimsPrincipal> CreateUserPrincipalAsync([NotNull] User user, CancellationToken token = default(CancellationToken));
		ValueTask<User> SignInAsync([NotNull] string userName, string password, bool lockoutOnFailure, CancellationToken token = default(CancellationToken));
		[ItemNotNull]
		ValueTask<User> SignInAsync([NotNull] User user, string password, bool lockoutOnFailure, CancellationToken token = default(CancellationToken));
		bool IsSignedIn([NotNull] ClaimsPrincipal principal);
		ValueTask<bool> CanSignInAsync([NotNull] User user, CancellationToken token = default(CancellationToken));
		Task RefreshSignInAsync([NotNull] User user, CancellationToken token = default(CancellationToken));
		ValueTask<User> ValidateSecurityStampAsync([NotNull] ClaimsPrincipal principal, CancellationToken token = default(CancellationToken));
		ValueTask<bool> ValidateSecurityStampAsync(User user, string securityStamp, CancellationToken token = default(CancellationToken));
		bool Like([NotNull] string userId, [NotNull] string recipientId);
		ValueTask<bool> LikeAsync([NotNull] string userId, [NotNull] string recipientId, CancellationToken token = default(CancellationToken));
		bool Unlike([NotNull] string userId, [NotNull] string recipientId);
		ValueTask<bool> UnlikeAsync([NotNull] string userId, [NotNull] string recipientId, CancellationToken token = default(CancellationToken));
		int Likes([NotNull] string userId);
		ValueTask<int> LikesAsync([NotNull] string userId, CancellationToken token = default(CancellationToken));
		int Likees([NotNull] string userId);
		ValueTask<int> LikeesAsync([NotNull] string userId, CancellationToken token = default(CancellationToken));
	}
}