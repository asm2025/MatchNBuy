using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using asm.Core.Web.Controllers;
using asm.Extensions;
using asm.Patterns.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using MatchNBuy.Model.Parameters;
using MatchNBuy.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace MatchNBuy.API.Controllers
{
	//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[AllowAnonymous]
	[Route("Users/{userId}/[controller]")]
	public class MessagesController : ApiController
	{
		private readonly IMessageRepository _messageRepository;
		private readonly IMapper _mapper;

		/// <inheritdoc />
		public MessagesController([NotNull] IMessageRepository messageRepository, [NotNull] IMapper mapper, ILogger<UsersController> logger)
			: base(logger)
		{
			_messageRepository = messageRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> List(string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			pagination ??= new MessageList();
			
			IQueryable<Message> queryable = _messageRepository.List(userId, pagination);
			pagination.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();

			IList<MessageForList> messages = await queryable.Paginate(pagination)
															.ProjectTo<MessageForList>(_mapper.ConfigurationProvider)
															.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<MessageForList>(messages, pagination));
		}

		[HttpGet("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Threads(string userId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			//if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			pagination ??= new MessageList();
			
			Paginated<MessageThread> threads = await _messageRepository.ThreadsAsync(userId, pagination, token);
			token.ThrowIfCancellationRequested();
			return Ok(threads);
		}

		[HttpGet("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> Thread(string userId, string recipientId, [FromQuery] SortablePagination pagination, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);
			if (string.IsNullOrEmpty(recipientId)) return BadRequest(recipientId);
			
			string claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!userId.IsSame(claimId) && !recipientId.IsSame(claimId)) return Unauthorized(userId);
			
			IQueryable<Message> queryable = _messageRepository.Thread(userId, recipientId, pagination);
			pagination.Count = await queryable.CountAsync(token);
			token.ThrowIfCancellationRequested();

			IList<MessageForList> messages = await queryable.Paginate(pagination)
															.ProjectTo<MessageForList>(_mapper.ConfigurationProvider)
															.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(new Paginated<MessageForList>(messages, pagination));
		}

		[HttpGet("{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Get(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();
			
			Message message = await _messageRepository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return Ok(messageForList);
		}

		[HttpPost("[action]")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.Created)]
		public async Task<IActionResult> Add(string userId, [FromBody][NotNull] MessageToAdd messageParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(userId) || !userId.IsSame(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return Unauthorized(userId);
			
			Message message = _mapper.Map<Message>(messageParams);
			message.SenderId = userId;
			message = await _messageRepository.AddAsync(message, token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception($"Add message for the user '{userId}' failed.");
			await _messageRepository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return CreatedAtAction(nameof(Get), new { id = message.Id }, messageForList);
		}

		[HttpPut("{id}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] MessageToEdit messageToParams, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return BadRequest();

			string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized(userId);

			Message message = await _messageRepository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			if (!message.SenderId.IsSame(userId)) return Unauthorized(userId);
			message = await _messageRepository.UpdateAsync(_mapper.Map(messageToParams, message), token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception("Updating message failed.");
			await _messageRepository.Context.SaveChangesAsync(token);
			token.ThrowIfCancellationRequested();
			
			MessageForList messageForList = _mapper.Map<MessageForList>(message);
			return Ok(messageForList);
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

			Message message = await _messageRepository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (message == null) return NotFound(id);
			if (!message.SenderId.IsSame(userId) && !User.IsInRole(Role.Administrators)) return Unauthorized(userId);
			message = await _messageRepository.DeleteAsync(message, token);
			token.ThrowIfCancellationRequested();
			if (message == null) throw new Exception("Deleting message failed.");
			await _messageRepository.Context.SaveChangesAsync(token);
			return Ok();
		}
	}
}