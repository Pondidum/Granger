using System;
using System.Net;

namespace Granger.ResponseHeaderValidation.Rules
{
	public class Http101Rule : HttpHeaderRule
	{
		public Http101Rule()
			: base(HttpStatusCode.SwitchingProtocols, "Upgrade", new Uri("https://tools.ietf.org/html/rfc7231#section-6.2.2"))
		{
		}
	}
}
