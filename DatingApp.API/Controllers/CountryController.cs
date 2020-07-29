﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DatingApp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using asm.Core.Web.Controllers;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Patterns.Sorting;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Data.Repositories;
using DatingApp.Model.TransferObjects;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DatingApp.API.Controllers
{
	[AllowAnonymous]
	[Route("[controller]")]
	public class CountryController : ApiController
	{
		private static readonly Lazy<ListSettings> __countriesSettings = new Lazy<ListSettings>(() => new ListSettings
		{
			PageSize = int.MaxValue,
			OrderBy = new[]
			{
				new SortField
				{
					Name = nameof(Country.Name),
					Type = SortType.Ascending
				}
			}
		});

		private readonly ICountryRepositoryBase _countryRepository;
		private readonly ICityRepositoryBase _cityRepository;
		private readonly IMapper _mapper;

		public CountryController([NotNull] ICountryRepositoryBase countryRepository, [NotNull] ICityRepositoryBase cityRepository, IMapper mapper, [NotNull] IConfiguration configuration, ILogger<WeatherController> logger)
			: base(configuration, logger)
		{
			_countryRepository = countryRepository;
			_cityRepository = cityRepository;
			_mapper = mapper;
		}

		[HttpGet]
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
		public async Task<IActionResult> Get(string code, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrWhiteSpace(code)) return BadRequest(code);
			Country country = await _countryRepository.GetAsync(token, code);
			token.ThrowIfCancellationRequested();
			if (country == null) return NotFound(code);
			CountryForList countryForList = _mapper.Map<CountryForList>(country);
			return Ok(countryForList);
		}

		[HttpGet("{code}/[action]")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> Cities(string code, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrWhiteSpace(code)) return NotFound();
			ListSettings listSettings = new ListSettings
			{
				PageSize = int.MaxValue,
				OrderBy = new[]
				{
					new SortField
					{
						Name = nameof(City.Name),
						Type = SortType.Ascending
					}
				},
				Filter = new DynamicFilter
				{
					Expression = $"{nameof(City.CountryCode)} == @0",
					Arguments = new object[]
					{
						code
					}
				}
			};

			IQueryable<City> queryable = _cityRepository.List(listSettings);
			IList<CityForList> cities = await queryable.ProjectTo<CityForList>(_mapper.ConfigurationProvider)
														.ToListAsync(token);
			token.ThrowIfCancellationRequested();
			return Ok(cities);
		}

		[HttpGet("/City/{id}")]
		[SwaggerResponse((int)HttpStatusCode.BadRequest)]
		[SwaggerResponse((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetCity(Guid id, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (id.IsEmpty()) return NotFound();
			City city = await _cityRepository.GetAsync(token, id);
			token.ThrowIfCancellationRequested();
			if (city == null) return NotFound(id);
			CityForList cityForList = _mapper.Map<CityForList>(city);
			return Ok(cityForList);
		}
	}
}
