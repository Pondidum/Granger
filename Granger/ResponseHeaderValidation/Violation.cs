using System.Collections.Generic;

namespace Granger.ResponseHeaderValidation
{
	public class Violation
	{
		public string Message { get; set; }
		public Dictionary<string, HrefWrapper> Links { get; set; }
	}
}