using System;
using System.Net;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http206Rule : HttpHeaderRule
	{
		public Http206Rule()
			: base(HttpStatusCode.PartialContent, "Content-Range", new Uri("http://tools.ietf.org/html/rfc7233#section-4.1"))
		{
		}
	}
}
