using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Web.Controllers;
using asm.Data.Patterns.Parameters;
using asm.Drawing.Helpers;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using MatchNBuy.Model.Parameters;
using MatchNBuy.Model.TransferObjects;
using JetBrains.Annotations;
using MatchNBuy.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Thread = MatchNBuy.Model.Thread;

namespace MatchNBuy.API.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ServiceFilter(typeof(LogUserActivity))]
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

		#region User
		[HttpGet]
		public async Task<IActionResult> List([FromQuery] UserList pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized();
			pagination ??= new UserList();

			ListSettings listSettings = _mapper.Map<ListSettings>(pagination);
			listSettings.Include = new List<string>
			{
				nameof(Model.User.Photos),
				nameof(Model.User.Likers),
				nameof(Model.User.Likees)
			};

			listSettings.FilterExpression = BuildFilter(userId, User, pagination);

			if (listSettings.OrderBy == null || listSettings.OrderBy.Count == 0)
			{
				listSettings.OrderBy = new[]
				{
					new SortField($"{nameof(Model.User.Likers)}.Count", SortType.Descending),
					new SortField(nameof(Model.User.LastActive), SortType.Descending),
					new SortField(nameof(Model.User.FirstName)),
					new SortField(nameof(Model.User.LastName))
				};
			}

			IQueryable<User> queryable = _repository.List(listSettings);
			listSettings.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();
			
			IList<UserForList> users = await queryable.Paginate(pagination)
													.ProjectTo<UserForList>(_mapper.ConfigurationProvider)
													.ToListAsync(token);

			if (users.Count > 0)
			{
				ISet<string> likees = await _repository.LikeesFromListAsync(userId, users.Select(e => e.Id), token);
				token.ThrowIfCancellationRequested();

				foreach (UserForList user in users)
				{
					bool isLikee = likees.Contains(user.Id);
					user.CanBeLiked = !isLikee;
					user.CanBeDisliked = isLikee;
					user.PhotoUrl = _repository.ImageBuilder.Build(user.Id, user.PhotoUrl).String();
				}
			}

			pagination = _mapper.Map(listSettings, pagination);
			return Ok(new Paginated<UserForList>(users,  pagination));

			static string BuildFilter(string userId, ClaimsPrincipal user, UserList pagination)
			{
				const int AGE_RANGE = 10;

				StringBuilder filter = new StringBuilder();
				filter.Append($"{nameof(Model.User.Id)} != \"{userId}\"");
				if (pagination.Gender.HasValue && pagination.Gender != Genders.NotSpecified) filter.Append($" && {nameof(Model.User.Gender)} == {(int)pagination.Gender.Value}");

				DateTime today = DateTime.Today;
				bool hasMinAge = pagination.MinAge.HasValue && pagination.MinAge > 0;
				bool hasMaxAge = pagination.MaxAge.HasValue && pagination.MaxAge > 0;

				// Set the default age if Likers and Likees are not specified
				if (!pagination.Likers && !pagination.Likees && (!hasMinAge || !hasMaxAge))
				{
					Claim dobClaim = user.FindFirst(ClaimTypes.DateOfBirth);

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

				// Set the default gender if Likers and Likees are not specified
				if (!pagination.Likers && !pagination.Likees && !pagination.Gender.HasValue)
				{
					Claim genderClaim = user.FindFirst(ClaimTypes.Gender);

					if (genderClaim != null && Enum.TryParse(genderClaim.Value, true, out Genders gender))
					{
						pagination.Gender = gender == Genders.Female
												? Genders.Male
												: Genders.Female;
					}
				}

				// Set the min age if it was explicitly set or Likers and Likees are not specified
				if (hasMinAge || !pagination.Likers && !pagination.Likees && pagination.MinAge > 0)
				{
					DateTime minDate = today.AddYears(-pagination.MinAge.Value);
					filter.Append($" && {nameof(Model.User.DateOfBirth)} <= DateTime({minDate.Year}, {minDate.Month}, {minDate.Day})");
				}

				// Set the max age if it was explicitly set or Likers and Likees are not specified
				if (hasMaxAge || !pagination.Likers && !pagination.Likees && pagination.MaxAge > 0)
				{
					DateTime maxDate = today.AddYears(-pagination.MaxAge.Value);
					filter.Append($" && {nameof(Model.User.DateOfBirth)} >= DateTime({maxDate.Year}, {maxDate.Month}, {maxDate.Day})");
				}

				// People who like me [RELEVANT TO THE USER IN THE LIST, working backwards]: LikerId = userId
				if (pagination.Likers) filter.Append($" && {nameof(Model.User.Likees)}.Any({nameof(Model.Like.LikeeId)} == \"{userId}\")");
				// People whom I like [RELEVANT TO THE USER IN THE LIST, working backwards]: LikeeId = userId
				if (pagination.Likees) filter.Append($" && {nameof(Model.User.Likers)}.Any({nameof(Model.Like.LikerId)} == \"{userId}\")");
				return filter.ToString();
			}
		}

		[HttpGet("{id}")]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Get([FromRoute] string id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(id)) return BadRequest(id);

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);

			bool isLikee = await _repository.IsLikeeAsync(user.Id, userId, token);
			token.ThrowIfCancellationRequested();
			
			UserForDetails userForDetails = _mapper.Map<UserForDetails>(user);
			userForDetails.CanBeLiked = !isLikee;
			userForDetails.CanBeDisliked = isLikee;
			userForDetails.PhotoUrl = _repository.ImageBuilder.Build(user.Id, user.PhotoUrl).String();
			return Ok(userForDetails);
		}

		[AllowAnonymous]
		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Login([FromBody][NotNull] UserForLogin loginParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			
			User user = await _repository.SignInAsync(loginParams.UserName, loginParams.Password, true, token);
			token.ThrowIfCancellationRequested();
			if (user == null || string.IsNullOrEmpty(user.Token)) return Unauthorized(loginParams.UserName);
			
			UserForLoginDisplay userForLoginDisplay = _mapper.Map<UserForLoginDisplay>(user);
			userForLoginDisplay.PhotoUrl = _repository.ImageBuilder.Build(user.Id, user.PhotoUrl).String();
			return Ok(new
			{
				token = user.Token,
				user = userForLoginDisplay
			});
		}

		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Logout(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			throw new NotImplementedException();
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
			userForSerialization.PhotoUrl = _repository.ImageBuilder.Build(user.Id, user.PhotoUrl).String();
			return CreatedAtAction(nameof(Get), new { id = user.Id }, userForSerialization);
		}

		[HttpGet("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Edit([FromRoute] string id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(id)) return BadRequest();
			if (!id.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(id);
			
			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);
			
			UserToUpdate userToUpdate = _mapper.Map<UserToUpdate>(user);
			return Ok(userToUpdate);
		}

		[HttpPut("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserToUpdate userParams, CancellationToken token)
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
		
			UserToUpdate userToUpdate = _mapper.Map<UserToUpdate>(user);
			return Ok(userToUpdate);
		}

		[HttpDelete("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(id)) return BadRequest();
			if (!id.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(id);
			
			User user = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (user == null) return NotFound(id);
			await _repository.DeleteAsync(user, token);
			return NoContent();
		}
		#endregion

		#region Photos
		[HttpGet("{userId}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Photos([FromRoute] string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId)) return BadRequest(userId);
			
			ListSettings listSettings = _mapper.Map<ListSettings>(pagination);
			StringBuilder filter = new StringBuilder();
			filter.Append($"{nameof(Photo.UserId)} == \"{userId}\"");
			listSettings.FilterExpression = filter.ToString();

			IQueryable<Photo> queryable = _repository.Photos.List(listSettings);
			pagination.Count = await queryable.LongCountAsync(token);
			token.ThrowIfCancellationRequested();
			
			IList<PhotoForList> photos = await queryable.Paginate(pagination)
														.ProjectTo<PhotoForList>(_mapper.ConfigurationProvider)
														.ToListAsync(token);
			token.ThrowIfCancellationRequested();

			foreach (PhotoForList photo in photos) 
				photo.Url = _repository.ImageBuilder.Build(userId, photo.Url).String();

			return Ok(new Paginated<PhotoForList>(photos, pagination));
		}

		[HttpGet("{userId}/Photos/{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetPhoto([FromRoute] string userId, Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			
			Photo photo = await _repository.Photos.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!userId.IsSame(photo.UserId)) return Unauthorized(userId);
			
			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			photoForList.Url = _repository.ImageBuilder.Build(userId, photo.Url).String();
			return Ok(photoForList);
		}

		[HttpPost("{userId}/Photos/Add")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> AddPhoto([FromRoute] string userId, [FromForm][NotNull] PhotoToAdd photoParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			if (photoParams.File == null || photoParams.File.Length == 0) throw new InvalidOperationException("No photo was provided to upload.");

			Stream stream = null;
			Image image = null;
			Image resizedImage = null;
			string fileName;

			try
			{
				string imagesPath = Path.Combine(Environment.ContentRootPath, _repository.ImageBuilder.BaseUri.Replace('/', '\\'), userId);
				fileName = Path.Combine(imagesPath, PathHelper.Extenstion(Path.GetFileName(photoParams.File.FileName), _repository.ImageBuilder.FileExtension));
				stream = photoParams.File.OpenReadStream();
				image = Image.FromStream(stream, true, true);
				(int x, int y) = asm.Numeric.Math.AspectRatio(image.Width, image.Height, Configuration.GetValue("images:users:size", 128));
				resizedImage = ImageHelper.Resize(image, x, y);
				fileName = ImageHelper.Save(resizedImage, fileName);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
			}
			finally
			{
				ObjectHelper.Dispose(ref stream);
				ObjectHelper.Dispose(ref image);
				ObjectHelper.Dispose(ref resizedImage);
			}

			if (string.IsNullOrEmpty(fileName)) throw new Exception($"Could not upload image for user '{userId}'.");

			Photo photo = _mapper.Map<Photo>(photoParams);
			photo.UserId = userId;
			photo.Url = Path.GetFileName(fileName);
			photo = await _repository.Photos.AddAsync(photo, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception($"Add photo '{fileName}' for the user '{userId}' failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			
			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			photoForList.Url = _repository.ImageBuilder.Build(userId, photo.Url).String();
			return CreatedAtAction(nameof(Get), new { id = photo.Id }, photoForList);
		}

		[HttpPut("{userId}/Photos/{id}/Update")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> UpdatePhoto([FromRoute] string userId, Guid id, [FromBody] PhotoToEdit photoToParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Photo photo = await _repository.Photos.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!photo.UserId.IsSame(userId)) return Unauthorized(userId);
			photo = await _repository.Photos.UpdateAsync(_mapper.Map(photoToParams, photo), token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception("Updating photo failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();

			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			photoForList.Url = _repository.ImageBuilder.Build(userId, photo.Url).String();
			return Ok(photoForList);
		}

		[HttpDelete("{userId}/Photos/{id}/Delete")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeletePhoto([FromRoute] string userId, Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			
			Photo photo = await _repository.Photos.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!photo.UserId.IsSame(userId) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			photo = await _repository.Photos.DeleteAsync(photo, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception("Deleting photo failed.");
			await _repository.Context.SaveChangesAsync(token);
			return NoContent();
		}

		[HttpGet("{userId}/Photos/Default")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetDefaultPhoto([FromRoute] string userId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Photo photo = await _repository.Photos.GetDefaultAsync(userId, token);
			if (photo == null) return NotFound(userId);
			if (!photo.UserId.IsSame(userId)) return Unauthorized(userId);

			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return Ok(photoForList);
		}

		[HttpPut("{userId}/Photos/{id}/SetDefault")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> SetDefaultPhoto([FromRoute] string userId, Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Photo photo = await _repository.Photos.GetAsync(new object[] {id}, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!photo.UserId.IsSame(userId)) return Unauthorized(userId);
			if (!await _repository.Photos.SetDefaultAsync(photo, token)) return NotFound(id);

			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return Ok(photoForList);
		}
		#endregion

		#region Messages
		[HttpGet("{userId}/Messages/[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Threads([FromRoute] string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			pagination ??= new MessageList();
			
			IQueryable<Thread> queryable = _repository.Messages.Threads(userId, pagination);
			pagination.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();

			IList<MessageThread> messageThreads = new List<MessageThread>(pagination.PageSize);

			await foreach (Thread thread in queryable.Paginate(pagination).AsAsyncEnumerable().WithCancellation(token))
			{
				MessageThread messageThread = _mapper.Map<MessageThread>(thread);
				messageThread.Participant = _mapper.Map<UserForLoginDisplay>(thread.RecipientId == userId
																				? thread.Recipient
																				: thread.Sender);
				messageThreads.Add(messageThread);
			}

			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<MessageThread>(messageThreads, pagination));
		}

		[HttpGet("{userId}/Messages/[action]/{threadId}")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Thread([FromRoute] string userId, [FromRoute] string threadId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);
			if (string.IsNullOrEmpty(threadId)) return BadRequest(threadId);
			
			string claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!userId.IsSame(claimId)) return Unauthorized(userId);
			
			IQueryable<Message> queryable = _repository.Messages.Thread(threadId, pagination);
			pagination.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();

			IList<MessageForList> messages = await queryable.Paginate(pagination)
															.ProjectTo<MessageForList>(_mapper.ConfigurationProvider)
															.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<MessageForList>(messages, pagination));
		}

		[HttpGet("{userId}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Messages([FromRoute] string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			pagination ??= new MessageList();
			
			IQueryable<Message> queryable = _repository.Messages.List(userId, pagination);
			pagination.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();

			IList<MessageForList> messages = await queryable.Paginate(pagination)
															.ProjectTo<MessageForList>(_mapper.ConfigurationProvider)
															.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<MessageForList>(messages, pagination));
		}

		[HttpGet("{userId}/Messages/{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetMessage([FromRoute] string userId, Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
	
			Message message = await _repository.Messages.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return Ok(messageForList);
		}

		[HttpPost("{userId}/Messages/Add")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> AddMessage([FromRoute] string userId, [FromBody][NotNull] MessageToAdd messageParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			
			Message message = _mapper.Map<Message>(messageParams);
			message.SenderId = userId;
			message = await _repository.Messages.AddAsync(message, token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception($"Add message for the user '{userId}' failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return CreatedAtAction(nameof(Thread), new { id = message.Id }, messageForList);
		}

		[HttpPost("{userId}/Messages/{id}/Reply")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> ReplyToMessage([FromRoute] string userId, Guid id, [FromBody][NotNull] MessageToEdit messageParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			
			Message message = await _repository.Messages.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);

			Message newMessage = await _repository.Messages.ReplyToAsync(userId, message, messageParams, token);
			token.ThrowIfCancellationRequested();
			if (newMessage == null) throw new Exception($"Reply to message for the user '{userId}' failed.");
			
			MessageForList messageForList = _mapper.Map<MessageForList>(newMessage);
			return CreatedAtAction(nameof(Thread), new
			{
				userId,
				threadId = message.ThreadId
			}, messageForList);
		}

		[HttpPut("{userId}/Messages/{id}/Update")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> UpdateMessage([FromRoute] string userId, Guid id, [FromBody] MessageToEdit messageToParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Message message = await _repository.Messages.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			if (!message.SenderId.IsSame(userId)) return Unauthorized(userId);
			message = await _repository.Messages.UpdateAsync(_mapper.Map(messageToParams, message), token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception("Updating message failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return Ok(messageForList);
		}

		[HttpDelete("{userId}/Messages/{id}/Delete")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DeleteMessage([FromRoute] string userId, Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Message message = await _repository.Messages.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			if (!message.SenderId.IsSame(userId) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			message = await _repository.Messages.DeleteAsync(message, token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception("Deleting message failed.");
			await _repository.Context.SaveChangesAsync(token);
			return NoContent();
		}

		[HttpPut("{userId}/Messages/{id}/{isRead}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> MarkMessage([FromRoute] string userId, Guid id, bool isRead, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			Message message = await _repository.Messages.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			if (!message.RecipientId.IsSame(userId)) return Unauthorized(userId);
			
			if (isRead)
				message.DateRead = DateTime.UtcNow;
			else
				message.DateRead = null;

			message = await _repository.Messages.UpdateAsync(message, token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception("Updating message failed.");
			await _repository.Context.SaveChangesAsync(token);
			return NoContent();
		}
		#endregion

		#region Likes
		[HttpPost("{userId}/[action]/{recipientId}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Like([FromRoute] string userId, [FromRoute] string recipientId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			if (string.IsNullOrEmpty(recipientId)) return BadRequest(recipientId);

			int likes = await _repository.LikeAsync(userId, recipientId, token);
			return Ok(likes);
		}

		[HttpDelete("{userId}/[action]/{recipientId}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Dislike([FromRoute] string userId, [FromRoute] string recipientId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			if (string.IsNullOrEmpty(recipientId)) return BadRequest(recipientId);

			int likes = await _repository.DislikeAsync(userId, recipientId, token);
			return Ok(likes);
		}

		[HttpGet("{userId}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Likes([FromRoute] string userId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			int count = await _repository.LikesAsync(userId, token);
			return Ok(count);
		}

		[HttpGet("{userId}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Likees([FromRoute] string userId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);

			int count = await _repository.LikeesAsync(userId, token);
			return Ok(count);
		}
		#endregion
	}
}