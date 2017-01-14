using System;
using Microsoft.Owin;
using System.Collections.Generic;
using System.Net;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http204Rule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode == (int)HttpStatusCode.NoContent)
			{
				if (response.ContentLength != null && response.ContentLength != 0)
					yield return new Violation
					{
						Message = "The Content-Length header should not be set when returning 204: No Content",
						Links = new Dictionary<string, HrefWrapper>
						{
							{ "rfc", HrefWrapper.From("http://tools.ietf.org/html/rfc7231#section-6.3.5") }
						}
					};

				var buffer = new byte[1];

				if (response.Body != null && response.Body.CanRead && response.Body.Read(buffer, 0, 1) > 0)
					yield return new Violation
					{
						Message = "There should not be any content when returning 204: No Content",
						Links = new Dictionary<string, HrefWrapper>
						{
							{ "rfc", HrefWrapper.From("http://tools.ietf.org/html/rfc7231#section-6.3.5") }
						}
					};
			}
		}
	}
}
