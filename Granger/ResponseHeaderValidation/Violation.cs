using System;
using System.Collections.Generic;

namespace Granger.ResponseHeaderValidation
{
	public class Violation
	{
		public string Message { get; set; }
		public Dictionary<string, HrefWrapper> Links { get; set; }

		public static Violation ForHttpHeader(string header, Uri rfcUri)
		{
			return new Violation
			{
				Message = "The response was missing a recommend header: " + header,
				Links = new Dictionary<string, HrefWrapper>
				{
					{ "rfc", HrefWrapper.From(rfcUri) }
				}
			};
		}
	}
}
