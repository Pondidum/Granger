using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http201Location : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode == (int)HttpStatusCode.Created && response.Headers.ContainsKey("Location") == false)
				yield return new Violation
				{
					Message = "The response was missing a recommend header: Location",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", new HrefWrapper { Href = new Uri("https://tools.ietf.org/html/rfc7231#section-6.3.2") } }
					}
				};
		}
	}
}
