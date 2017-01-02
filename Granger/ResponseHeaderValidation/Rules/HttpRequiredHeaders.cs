using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class HttpRequiredHeaders : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			var headers = response.Headers;

			if (headers.ContainsKey("Content-Length") == false)
				yield return ViolationFor("Content-Length", "https://tools.ietf.org/html/rfc7230#section-3.3.2");

			if (headers.ContainsKey("Content-Type") == false)
				yield return ViolationFor("Content-Type", "https://tools.ietf.org/html/rfc7231#section-3.1.1.5");
		}

		private static Violation ViolationFor(string header, string rfcUri)
		{
			return new Violation
			{
				Message = $"The response was missing a recommend header: {header}",
				Links = new Dictionary<string, HrefWrapper>
				{
					{ "rfc", new HrefWrapper { Href = new Uri(rfcUri) } }
				}
			};
		}
	}
}
