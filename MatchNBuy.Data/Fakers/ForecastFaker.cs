using System;
using System.Collections.Generic;
using asm.Collections;
using asm.Helpers;
using Bogus;
using MatchNBuy.Model;

namespace MatchNBuy.Data.Fakers
{
	public class ForecastFaker : Faker<Forecast>
	{
		private static readonly IReadOnlyRangeDictionary<int, (string Name, string Description)> __summaries = new ReadOnlyRangeDictionary<int, (string Name, string Description)>(new Dictionary<(int, int), (string Name, string Description)>
		{
			{ (int.MinValue, 0), ("Freezing", "A blast of Arctic air is sweeping the country with below-freezing temperatures.") }
			, { (1, 5), ("Bracing", "Bracing for snow today with weather charts forecasting freezing temperatures to whip across the country.") }
			, { (6, 9), ("Chilly", "Cold and cloudy with a sensation of sheerness.") }
			, { (10, 19), ("Cool", "Moderate coldness with partly cloudy gray skies.") }
			, { (20, 29), ("Warm", "Warm and sunny blue skies.") }
			, { (30, int.MaxValue), ("Hot", "Hot weather with clear skies.") }
		});

		/// <inheritdoc />
		public ForecastFaker()
		{
			base.RuleFor(e => e.Date, DateTime.Today);
			base.RuleFor(e => e.TemperatureC, () => RNGRandomHelper.Next(-20, 50));
			base.RuleFor(e => e.Keyword, (f, e) => __summaries[e.TemperatureC].Name);
			base.RuleFor(e => e.Summary, (f, e) => __summaries[e.TemperatureC].Description);
		}
	}
}