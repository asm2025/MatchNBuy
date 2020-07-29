﻿using System;
using System.Collections.Generic;
using asm.Collections;
using asm.Helpers;
using asm.Patterns.Images;
using Bogus;
using DatingApp.Model;

namespace DatingApp.Data.Fakers
{
	public class ForecastFaker : Faker<Forecast>
	{
		private static readonly IReadOnlyRangeDictionary<int, (string Name, string Description)> __summaries = new ReadOnlyRangeDictionary<int, (string Name, string Description)>(new Dictionary<(int, int), (string Name, string Description)>
		{
			{ (int.MinValue, 0), ("Freezing", "A blast of Arctic air is sweeping the country with below-freezing temperatures.") }
			, { (1, 5), ("Bracing", "Bracing for snow today with weather charts forecasting freezing temperatures to whip across the country.") }
			, { (6, 9), ("Chilly", "Cold and cloudy with a sensation of sheerness.") }
			, { (10, 14), ("Mild", "Light breeze of wind with mildly cold temperatures.") }
			, { (15, 19), ("Cool", "Moderate coldness with partly cloudy gray skies.") }
			, { (20, 27), ("Warm", "Warm and sunny blue skies.") }
			, { (28, 33), ("Hot", "Hot weather with clear skies.") }
			, { (34, int.MaxValue), ("Scorching", "It feels like an oven!") }
		});

		/// <inheritdoc />
		public ForecastFaker(IImageBuilder imageBuilder)
		{
			base.RuleFor(e => e.Date, DateTime.Today);
			base.RuleFor(e => e.TemperatureC, () => RNGRandomHelper.Next(-20, 50));
			base.RuleFor(e => e.ImageUrl, (f, e) => imageBuilder?.Build(__summaries[e.TemperatureC].Name)?.ToString());
			base.RuleFor(e => e.Summary, (f, e) => __summaries[e.TemperatureC].Description);
		}
	}
}