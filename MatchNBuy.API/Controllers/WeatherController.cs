﻿using System;
using System.Collections.Generic;
using essentialMix.Core.Web.Controllers;
using JetBrains.Annotations;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchNBuy.API.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public class WeatherController : ApiController
{
	private readonly IForecastRepository _repository;

	public WeatherController([NotNull] IForecastRepository repository, [NotNull] IConfiguration configuration, ILogger<WeatherController> logger)
		: base(configuration, logger)
	{
		_repository = repository;
	}

	[HttpGet]
	public IActionResult List(DateTime date)
	{
		if (date == DateTime.MinValue || date == DateTime.MaxValue) date = DateTime.Today;
		IList<Forecast> forecasts = _repository.List(date);
		return Ok(new
		{
			selectedDate = date,
			forecasts
		});
	}

	[HttpGet("{date}")]
	public IActionResult Get([FromRoute] DateTime date)
	{
		Forecast result = _repository.Get(date);
		return Ok(result);
	}
}