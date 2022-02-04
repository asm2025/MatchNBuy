﻿using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using MatchNBuy.Data.Fakers;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model.TransferObjects;
using Swashbuckle.AspNetCore.Filters;
using User = MatchNBuy.Model.User;

namespace MatchNBuy.API.Swagger.Examples;

public class UserToRegisterExample : IExamplesProvider<UserToRegister>
{
	private readonly UserFaker _faker;
	private readonly IMapper _mapper;

	public UserToRegisterExample([NotNull] ICityRepositoryBase repository, [NotNull] IMapper mapper)
	{
		_faker = new UserFaker(repository.List().ToList());
		_mapper = mapper;
	}

	/// <inheritdoc />
	public UserToRegister GetExamples()
	{
		User user = _faker.Generate();
		return _mapper.Map<UserToRegister>(user);
	}
}