using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	public class PhotoToAdd : PhotoToEdit
	{
		[Required]
		public IFormFile File { get; set; }
	}
}