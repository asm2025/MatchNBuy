using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class PhotoToAdd : PhotoToEdit
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		public IFormFile File { get; set; }
	}
}