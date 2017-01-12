using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation
{
	public class HttpHeaderRule : IResponseRule
	{
		private readonly HttpStatusCode _forStatus;
		private readonly IDictionary<string, Uri> _headersAndRfcs;

		public HttpHeaderRule(HttpStatusCode forStatus, string mandatoryHeader, Uri headerRfc)
			: this(forStatus, new Dictionary<string, Uri> { { mandatoryHeader, headerRfc } })
		{
		}

		public HttpHeaderRule(HttpStatusCode forStatus, IDictionary<string, Uri> headersAndRfcs)
		{
			_forStatus = forStatus;
			_headersAndRfcs = headersAndRfcs;
		}

		public IEnumerable<Violation> GetViolations(IOwinResponse response)
		{
			if (response.StatusCode != (int)_forStatus)
				yield break;

			foreach (var pair in _headersAndRfcs)
				if (response.Headers.ContainsKey(pair.Key) == false)
					yield return Violation.ForHttpHeader(pair.Key, pair.Value);
		}
	}
}
