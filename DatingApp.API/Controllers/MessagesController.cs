using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Web.Controllers;
using asm.Extensions;
using asm.Patterns.Pagination;
using AutoMapper;
using DatingApp.Data.Repositories;
using DatingApp.Model;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace DatingApp.API.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("users/{userId}/[controller]")]
	public class MessagesController : ApiController
	{
		private readonly IMessageRepository _repository;
		private readonly IMapper _mapper;

		/// <inheritdoc />
		public MessagesController([NotNull] IMessageRepository repository, [NotNull] IMapper mapper, ILogger<UsersController> logger)
			: base(logger)
		{
			_repository = repository;
			_mapper = mapper;
		}

		[HttpGet]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> List(string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized(userId);

			Paginated<MessageThread> threads = await _repository.ListThreadsAsync(userId, pagination, token);
			token.ThrowIfCancellationRequested();
			return Ok(threads);
		}

		[HttpGet("{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Get(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			Message message = await _repository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return Ok(messageForList);
		}

		//[HttpPost("[action]")]
		//[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		//[SwaggerResponse((int)HttpStatusCode.Created)]
		//public async Task<IActionResult> Add(string userId, [FromForm][NotNull] MessageToAdd messageParams, CancellationToken token)
		//{
		//	token.ThrowIfCancellationRequested();
		//	if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized(userId);
		//	if (await _userRepository.GetAsync(token, messageParams.RecipientId) == null) return NotFound(messageParams.RecipientId);

		//	Message message = _mapper.Map<Message>(messageParams);
		//	message.SenderId = userId;
		//	message = await _repository.AddAsync(message, token);
		//	token.ThrowIfCancellationRequested();
		//	if (message == null) throw new Exception($"Add message for the user '{userId}' failed.");
		//	await _repository.Context.SaveChangesAsync(token);
		//	token.ThrowIfCancellationRequested();
		//	MessageForList messageForList = _mapper.Map<MessageForList>(message);
		//	return CreatedAtAction(nameof(Get), new { id = message. }, messageForList);
		//}

		//[HttpPut("{id}/[action]")]
		//[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		//[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		//[SwaggerResponse((int)HttpStatusCode.NotFound)]
		//public async Task<IActionResult> Update(string userId, Guid id, [FromBody] MessageToEdit messageToParams, CancellationToken token)
		//{
		//	token.ThrowIfCancellationRequested();
		//	if (id.IsEmpty()) return BadRequest();
		//	bool isAdmin = User.IsInRole(Role.Administrators);
		//	if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier).Value) && !isAdmin) return Unauthorized(userId);
		//	Message message = await _repository.GetAsync(token, id);
		//	token.ThrowIfCancellationRequested();
		//	if (message == null) return NotFound(id);
		//	if (!message.UserId.IsSame(userId)) return Unauthorized(userId);
		//	message = await _repository.UpdateAsync(_mapper.Map(messageToParams, message), token);
		//	token.ThrowIfCancellationRequested();
		//	if (message == null) throw new Exception("Updating message failed.");
		//	await _repository.Context.SaveChangesAsync(token);
		//	token.ThrowIfCancellationRequested();
		//	MessageForList messageForList = _mapper.Map<MessageForList>(message);
		//	return Ok(messageForList);
		//}

		//[HttpDelete("{id}/[action]")]
		//[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		//[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		//[SwaggerResponse((int)HttpStatusCode.NotFound)]
		//public async Task<IActionResult> Delete(string userId, Guid id, CancellationToken token)
		//{
		//	token.ThrowIfCancellationRequested();
		//	if (id.IsEmpty()) return BadRequest();
		//	bool isAdmin = User.IsInRole(Role.Administrators);
		//	if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier).Value) && !isAdmin) return Unauthorized(userId);
		//	Message message = await _repository.GetAsync(token, id);
		//	token.ThrowIfCancellationRequested();
		//	if (message == null) return NotFound(id);
		//	if (!message.UserId.IsSame(userId)) return Unauthorized(userId);
		//	message = await _repository.DeleteAsync(message, token);
		//	token.ThrowIfCancellationRequested();
		//	if (message == null) throw new Exception("Deleting message failed.");
		//	await _repository.Context.SaveChangesAsync(token);
		//	token.ThrowIfCancellationRequested();
		//	MessageForList messageForList = _mapper.Map<MessageForList>(message);
		//	return Ok(messageForList);
		//}
	}
}