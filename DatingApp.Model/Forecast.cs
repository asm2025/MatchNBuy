using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using asm.Data.Model;

namespace DatingApp.Model
{
	[DebuggerDisplay("{Date} {TemperatureC}")]
	[Serializable]
	public class Forecast : IEntity
	{
		[Key]
		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

		public string ImageUrl { get; set; }

		public string Summary { get; set; }
	}
}
