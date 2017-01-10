using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http101Rule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode == (int)HttpStatusCode.SwitchingProtocols && response.Headers.ContainsKey("Upgrade") == false)
				yield return Violation.ForHttpHeader(
					"Upgrade",
					new Uri("https://tools.ietf.org/html/rfc7231#section-6.2.2")
				);
		}
	}
}
