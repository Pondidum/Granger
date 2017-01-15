using System.Collections.Generic;
using System.Net;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http205Rule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode != (int)HttpStatusCode.NoContent)
				yield break;

			if (response.ContentLength != null && response.ContentLength != 0)
				yield return new Violation
				{
					Message = "The Content-Length header must not be set when returning 205: Reset Content",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", HrefWrapper.From("https://tools.ietf.org/html/rfc7231#section-6.3.6") }
					}
				};

			var buffer = new byte[1];

			if (response.Body != null && response.Body.CanRead && response.Body.Read(buffer, 0, 1) > 0)
				yield return new Violation
				{
					Message = "There must not be any content when returning 205: Reset Content",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", HrefWrapper.From("https://tools.ietf.org/html/rfc7231#section-6.3.6") }
					}
				};
		}
	}
}
