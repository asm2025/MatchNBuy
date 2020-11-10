using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class PhotoToAdd : PhotoToEdit
	{
		public IFormFile File { get; set; }
	}
}