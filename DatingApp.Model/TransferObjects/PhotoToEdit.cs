using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	public class PhotoToEdit
	{
		[StringLength(512)]
		public string Description { get; set; }
		
		public bool IsDefault { get; set; }
	}
}