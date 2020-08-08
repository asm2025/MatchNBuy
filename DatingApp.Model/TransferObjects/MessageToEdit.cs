using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Model.TransferObjects
{
	[Serializable]
	public class MessageToEdit
	{
		[Required]
		[StringLength(512, MinimumLength = 1)]
		public string Content { get; set; }
	}
}