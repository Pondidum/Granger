using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class HttpRequiredHeadersRule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			var headers = response.Headers;

			if (headers.ContainsKey("Content-Length") == false)
				yield return Violation.ForHttpHeader(
					"Content-Length",
					new Uri("https://tools.ietf.org/html/rfc7230#section-3.3.2")
				);

			if (headers.ContainsKey("Content-Type") == false)
				yield return Violation.ForHttpHeader(
					"Content-Type",
					new Uri("https://tools.ietf.org/html/rfc7231#section-3.1.1.5")
				);
		}
	}
}
