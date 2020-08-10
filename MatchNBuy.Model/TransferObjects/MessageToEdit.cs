using System;
using System.ComponentModel.DataAnnotations;

namespace MatchNBuy.Model.TransferObjects
{
	[Serializable]
	public class MessageToEdit
	{
		[Required]
		[StringLength(512)]
		public string Content { get; set; }
	}
}