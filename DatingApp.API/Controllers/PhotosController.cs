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
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Data.ImageBuilders;
using DatingApp.Data.Repositories;
using DatingApp.Model;
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
	[Route("users/{userId}/[controller]")]
	public class PhotosController : ApiController
	{
		private readonly IPhotoRepository _repository;
		private readonly IUserImageBuilder _userImageBuilder;
		private readonly IMapper _mapper;

		/// <inheritdoc />
		public PhotosController([NotNull] IPhotoRepository repository, [NotNull] IUserImageBuilder userImageBuilder, [NotNull] IMapper mapper, ILogger<UsersController> logger)
			: base(logger)
		{
			_repository = repository;
			_userImageBuilder = userImageBuilder;
			_mapper = mapper;
		}

		[HttpGet]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> List(string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			
			ListSettings listSettings = _mapper.Map<ListSettings>(pagination);
			StringBuilder filter = new StringBuilder();
			IList<object> args = new List<object>();
			filter.Append($"{nameof(Photo.UserId)} == @{args.Count}");
			args.Add(userId);

			listSettings.Filter = new DynamicFilter
			{
				Expression = filter.ToString(),
				Arguments = args.ToArray()
			};

			IQueryable<Photo> queryable = _repository.List(listSettings);
			pagination.Count = await queryable.LongCountAsync(token);
			token.ThrowIfCancellationRequested();
			IList<PhotoForList> photos = await queryable.Paginate(pagination)
														.ProjectTo<PhotoForList>(_mapper.ConfigurationProvider)
														.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<PhotoForList>(photos, pagination));
		}

		[HttpGet("{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Get(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			Photo photo = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return Ok(photoForList);
		}

		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> Add(string userId, [FromForm][NotNull] PhotoToAdd photoParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (photoParams.File == null || photoParams.File.Length == 0) throw new InvalidOperationException("No photo was provided to upload.");
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);

			Stream stream = null;
			Image image = null;
			Image resizedImage = null;
			string fileName;

			try
			{
				string imagesPath = Path.Combine(Environment.ContentRootPath, _userImageBuilder.BaseUri.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), userId);
				fileName = Path.Combine(imagesPath, PathHelper.Extenstion(Path.GetFileName(photoParams.File.FileName), _userImageBuilder.FileExtension));
				stream = photoParams.File.OpenReadStream();
				image = Image.FromStream(stream, true, true);
				(int x, int y) = asm.Numeric.Math.AspectRatio(image.Width, image.Height, Configuration.GetValue("images:users:size", 128));
				resizedImage = ImageHelper.Resize(image, x, y);
				fileName = ImageHelper.Save(resizedImage, fileName);
			}
			finally
			{
				ObjectHelper.Dispose(ref stream);
				ObjectHelper.Dispose(ref image);
				ObjectHelper.Dispose(ref resizedImage);
			}

			if (string.IsNullOrEmpty(fileName)) throw new Exception($"Could not upload image for user '{userId}'.");
			fileName = $"{userId}/{Path.GetFileNameWithoutExtension(fileName)}";
			Photo photo = _mapper.Map<Photo>(photoParams);
			photo.UserId = userId;
			photo.Url = _userImageBuilder.Build(fileName).ToString();
			photo = await _repository.AddAsync(photo, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception($"Add photo '{fileName}' for the user '{userId}' failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return CreatedAtAction(nameof(Get), new { id = photo.Id }, photoForList);
		}

		[HttpPut("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] PhotoToEdit photoToParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);

			Photo photo = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!photo.UserId.IsSame(userId) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			photo = await _repository.UpdateAsync(_mapper.Map(photoToParams, photo), token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception("Updating photo failed.");
			await _repository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();

			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return Ok(photoForList);
		}

		[HttpDelete("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);
			
			Photo photo = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			if (!photo.UserId.IsSame(userId) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			photo = await _repository.DeleteAsync(photo, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) throw new Exception("Deleting photo failed.");
			await _repository.Context.SaveChangesAsync(token);
			return Ok();
		}

		[HttpGet("[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetDefault(string userId, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);

			Photo photo = await _repository.GetDefaultAsync(userId, token);
			if (photo == null) return NotFound(userId);

			PhotoForList photoForList = _mapper.Map<PhotoForList>(photo);
			return Ok(photoForList);
		}

		[HttpPut("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> SetDefault(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);
			Photo photo = await _repository.GetAsync(new object[] {id}, token);
			token.ThrowIfCancellationRequested();
			if (photo == null) return NotFound(id);
			bool isAdmin = User.IsInRole(Role.Administrators);
			if (!photo.UserId.IsSame(userId) && !isAdmin) return Unauthorized(userId);
			if (!await _repository.SetDefaultAsync(photo, token)) return NotFound(id);
			return Ok();
		}
	}
}