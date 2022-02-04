using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Web.Controllers;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Patterns.Sorting;
using JetBrains.Annotations;
using MatchNBuy.Data.Repositories;
using MatchNBuy.Model;
using MatchNBuy.Model.TransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace MatchNBuy.API.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public class CountriesController : ApiController
{
	private static readonly Lazy<ListSettings> __countriesSettings = new Lazy<ListSettings>(() => new ListSettings
	{
		PageSize = int.MaxValue,
		OrderBy = new[] { new SortField(nameof(Country.Name)) }
	});

	private readonly ICountryRepositoryBase _countryRepository;
	private readonly ICityRepositoryBase _cityRepository;
	private readonly IMapper _mapper;

	public CountriesController([NotNull] ICountryRepositoryBase countryRepository, [NotNull] ICityRepositoryBase cityRepository, IMapper mapper, [NotNull] IConfiguration configuration, ILogger<WeatherController> logger)
		: base(configuration, logger)
	{
		_countryRepository = countryRepository;
		_cityRepository = cityRepository;
		_mapper = mapper;
	}

	[HttpGet]
	[ItemNotNull]
	public async Task<IActionResult> List(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		IQueryable<Country> queryable = _countryRepository.List(__countriesSettings.Value);
		IList<CountryForList> countries = await queryable.ProjectTo<CountryForList>(_mapper.ConfigurationProvider)
														.ToListAsync(token);
		token.ThrowIfCancellationRequested();
		return Ok(countries);
	}

	[HttpGet("{code}")]
	[SwaggerResponse((int)HttpStatusCode.BadRequest)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[ItemNotNull]
	public async Task<IActionResult> Get([FromRoute] string code, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrWhiteSpace(code)) return BadRequest(code);
		Country country = await _countryRepository.GetAsync(code.ToUpperInvariant(), token);
		token.ThrowIfCancellationRequested();
		if (country == null) return NotFound(code);
		CountryForList countryForList = _mapper.Map<CountryForList>(country);
		return Ok(countryForList);
	}

	[HttpGet("{code}/[action]")]
	[SwaggerResponse((int)HttpStatusCode.BadRequest)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[ItemNotNull]
	public async Task<IActionResult> Cities([FromRoute] string code, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrWhiteSpace(code)) return NotFound();
		ListSettings listSettings = new ListSettings
		{
			PageSize = int.MaxValue,
			OrderBy = new[] { new SortField(nameof(City.Name)) },
			FilterExpression = $"{nameof(City.CountryCode)} == \"{code.ToUpperInvariant()}\""
		};

		IQueryable<City> queryable = _cityRepository.List(listSettings);
		IList<CityForList> cities = await queryable.ProjectTo<CityForList>(_mapper.ConfigurationProvider)
													.ToListAsync(token);
		token.ThrowIfCancellationRequested();
		return Ok(cities);
	}

	[HttpGet("[action]/{id}")]
	[SwaggerResponse((int)HttpStatusCode.BadRequest)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[ItemNotNull]
	public async Task<IActionResult> Cities(Guid id, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (id.IsEmpty()) return NotFound();
		City city = await _cityRepository.GetAsync(id, token);
		token.ThrowIfCancellationRequested();
		if (city == null) return NotFound(id);
		CityForList cityForList = _mapper.Map<CityForList>(city);
		return Ok(cityForList);
	}
}