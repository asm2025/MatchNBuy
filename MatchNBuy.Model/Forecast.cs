using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using asm.Data.Model;
using JetBrains.Annotations;

namespace MatchNBuy.Model
{
	[DebuggerDisplay("{Date} {TemperatureC}")]
	[Serializable]
	public class Forecast : IEntity
	{
		[Key]
		public DateTime Date { get; set; }

		[NotNull]
		public string DayName => Date.ToString("ddd");

		public string Keyword { get; set; }
		
		public int TemperatureC { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

		public string Summary { get; set; }
	}
}
