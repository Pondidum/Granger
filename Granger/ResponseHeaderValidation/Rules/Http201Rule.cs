using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http201Rule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode == (int)HttpStatusCode.Created && response.Headers.ContainsKey("Location") == false)
				yield return Violation.ForHttpHeader(
					"Location",
					new Uri("https://tools.ietf.org/html/rfc7231#section-6.3.2")
				);
		}
	}
}
