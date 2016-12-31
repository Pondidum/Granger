using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class ContentTypeRule : IResponseRule
	{
		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (string.IsNullOrWhiteSpace(response.ContentType))
				yield return new Violation
				{
					Message = "The response was missing a recommend header: Content-Type",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", new HrefWrapper { Href = new Uri("https://tools.ietf.org/html/rfc2616#section-7.2.1") } }
					}
				};
		}
	}
}
