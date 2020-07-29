﻿using asm.Core.Web.Controllers;
using AutoMapper;
using DatingApp.Data.Repositories;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DatingApp.API.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("users/{userId}/[controller]")]
	public class LikesController : ApiController
	{
		private readonly ILikeRepository _repository;
		private readonly IMapper _mapper;

		/// <inheritdoc />
		public LikesController([NotNull] ILikeRepository repository, [NotNull] IMapper mapper, ILogger<UsersController> logger)
			: base(logger)
		{
			_repository = repository;
			_mapper = mapper;
		}
	}
}