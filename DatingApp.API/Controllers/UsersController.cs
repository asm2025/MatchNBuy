using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Web.Controllers;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Data.Repositories;
using DatingApp.Model;
using DatingApp.Model.Parameters;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace DatingApp.API.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("[controller]")]
	public class UsersController : ApiController
	{
		private readonly IUserRepository _repository;
		private readonly IMapper _mapper;

		/// <inheritdoc />
		public UsersController([NotNull] IUserRepository repository, [NotNull] IMapper mapper, [NotNull] IConfiguration configuration, ILogger<UsersController> logger)
			: base(configuration, logger)
		{
			_repository = repository;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> List([FromQuery] UserList pagination, CancellationToken token)
		{
			const int AGE_RANGE = 10;

			token.ThrowIfCancellationRequested();
			pagination ??= new UserList();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			ListSettings listSettings = _mapper.Map<ListSettings>(pagination);
			listSettings.Include = new[]
			{
				"Photos"
			};

			StringBuilder filter = new StringBuilder();
			IList<object> args = new List<object>();
			filter.Append($"{nameof(Model.User.Id)} != @{args.Count}");
			args.Add(userId);
			
			if (pagination.Gender.HasValue && pagination.Gender != Genders.NotSpecified)
			{
				filter.Append($" and {nameof(Model.User.Gender)} == @{args.Count}");
				args.Add((int)pagination.Gender.Value);
			}

			DateTime today = DateTime.Today;
			bool hasMinAge = pagination.MinAge.HasValue && pagination.MinAge > 0;
			bool hasMaxAge = pagination.MaxAge.HasValue && pagination.MaxAge > 0;

			if (!pagination.Likers && !pagination.Likees && (!hasMinAge || !hasMaxAge))
			{
				Claim dobClaim = User.FindFirst(ClaimTypes.DateOfBirth);

				if (dobClaim != null && DateTime.TryParse(dobClaim.Value, out DateTime dob))
				{
					double age = DateTime.Today.Years(dob).NotBelow(Model.User.AGE_MIN);
					if (!hasMinAge) pagination.MinAge = ((int)(age - AGE_RANGE)).NotBelow(Model.User.AGE_MIN);
					if (!hasMaxAge) pagination.MaxAge = (int)(age + AGE_RANGE);
				}
				else
				{
					if (!hasMinAge) pagination.MinAge = Model.User.AGE_MIN;
					if (!hasMaxAge) pagination.MaxAge = Model.User.AGE_MAX;
				}

				if (pagination.MaxAge < pagination.MinAge) pagination.MaxAge = pagination.MinAge.Value + AGE_RANGE;
			}

			if (!pagination.Likers && !pagination.Likees && !pagination.Gender.HasValue)
			{
				Claim genderClaim = User.FindFirst(ClaimTypes.Gender);

				if (genderClaim != null && Enum.TryParse(genderClaim.Value, true, out Genders gender))
				{
					pagination.Gender = gender == Genders.Female
											? Genders.Male
											: Genders.Female;
				}
			}

			if (pagination.MinAge > 0)
			{
				DateTime minDate = today.AddYears(-pagination.MinAge.Value);
				filter.Append($" and {nameof(Model.User.DateOfBirth)} <= DateTime({minDate.Year}, {minDate.Month}, {minDate.Day})");
			}

			if (pagination.MaxAge > 0)
			{
				DateTime maxDate = today.AddYears(-pagination.MaxAge.Value);
				filter.Append($" and {nameof(Model.User.DateOfBirth)} >= DateTime({maxDate.Year}, {maxDate.Month}, {maxDate.Day})");
			}

			if (pagination.Likers)
			{
				filter.Append($" and {nameof(Model.User.Likers)}.Contains(@{args.Count})");
				args.Add(userId);
			}

			if (pagination.Likees)
			{
				filter.Append($" and {nameof(Model.User.Likees)}.Contains(@{args.Count})");
				args.Add(userId);
			}

			listSettings.Filter = new DynamicFilter
			{
				Expression = filter.ToString(),
				Arguments = args.ToArray()
			};

			if (listSettings.OrderBy == null || listSettings.OrderBy.Count == 0)
			{
				listSettings.OrderBy = new[]
				{
					new SortField(nameof(Model.User.LastActive), SortType.Descending),
					new SortField(nameof(Model.User.FirstName)),
					new SortField(nameof(Model.User.LastName))
				};
			}

			IQueryable<User> queryable = _repository.List(listSettings);
			listSettings.Count = await queryable.LongCountAsync(token);
			token.ThrowIfCancellationRequested();
			IList<UserForList> users = await queryable.Paginate(pagination)
													.ProjectTo<UserForList>(_mapper.ConfigurationProvider)
													.ToListAsync(token);
			return Ok(new Paginated<UserForList>(users, pagination));
		}

		[HttpGet("{id}")]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Get(string id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);
			UserForList userForList = _mapper.Map<UserForList>(user);
			return Ok(userForList);
		}

		[AllowAnonymous]
		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Login([FromBody][NotNull] UserForLogin loginParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			string userToken = await _repository.SignInAsync(loginParams.UserName, loginParams.Password, true, token);
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userToken)) return Unauthorized(loginParams.UserName);
			return Ok(userToken);
		}

		[AllowAnonymous]
		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> Register([FromBody][NotNull] UserToRegister userParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			User user = _mapper.Map<User>(userParams);
			user = await _repository.AddAsync(user, userParams.Password, token);
			token.ThrowIfCancellationRequested();
			UserForSerialization userForSerialization = _mapper.Map<UserForSerialization>(user);
			return CreatedAtAction(nameof(Get), new { id = user.Id }, userForSerialization);
		}

		[HttpPut("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Update(string id, [FromBody] UserToUpdate userParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(id)) return BadRequest();
			if (!id.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(id);
			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);
			user = _mapper.Map(userParams, user);
			user.Id = id;
			user = await _repository.UpdateAsync(user, token);
			token.ThrowIfCancellationRequested();
			if (user == null) throw new Exception("Updating user failed.");
			UserForSerialization userForSerialization = _mapper.Map<UserForSerialization>(user);
			return Ok(userForSerialization);
		}

		[HttpDelete("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Delete(string id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(id)) return BadRequest();
			if (!id.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(id);
			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);
			await _repository.DeleteAsync(user, token);
			return Ok();
		}
	}
}