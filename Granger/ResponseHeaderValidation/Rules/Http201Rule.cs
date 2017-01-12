using System;
using System.Net;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http201Rule : HttpHeaderRule
	{
		public Http201Rule()
			: base(HttpStatusCode.Created, "Location", new Uri("https://tools.ietf.org/html/rfc7231#section-6.3.2"))
		{
		}
	}
}
