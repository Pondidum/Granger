using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class ResponseHeaderValidatorMiddleware
	{
		private readonly AppFunc _next;
		private readonly List<IResponseRule> _rules;

		public ResponseHeaderValidatorMiddleware(AppFunc next)
		{
			_next = next;
			_rules = new List<IResponseRule>
			{
				new ContentTypeRule(),
				new Http201Location()
			};
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			await _next.Invoke(environment);

			var context = new OwinContext(environment);
			var response = context.Response;

			var violations = _rules.SelectMany(rule => rule.GetViolations(response)).ToArray();

			if (violations.Any())
			{
				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				response.ContentType = "application/json";

				await response.WriteAsync(JsonConvert.SerializeObject(violations));
			}

			await Task.Yield();
		}

		private class ContentTypeRule : IResponseRule
		{
			public IEnumerable<Violation> GetViolations(IOwinResponse response)
			{
				if (string.IsNullOrWhiteSpace(response.ContentType))
					yield return new Violation
					{
						Message = "The response was missing a recommend header: Content-Type",
						Links = new Dictionary<string, HrefWrapper>
						{
							{ "rfc", new HrefWrapper { href = "https://tools.ietf.org/html/rfc2616#section-7.2.1" } }
						}
					};
			}
		}

		private class Http201Location : IResponseRule
		{
			public IEnumerable<Violation> GetViolations(IOwinResponse response)
			{
				if (response.StatusCode == (int)HttpStatusCode.Created && response.Headers.ContainsKey("Location") == false)
					yield return new Violation
					{
						Message = "The response was missing a recommend header: Location",
						Links = new Dictionary<string, HrefWrapper>
						{
							{ "rfc", new HrefWrapper { href = "https://tools.ietf.org/html/rfc7231#section-6.3.2" } }
						}
					};
			}
		}

		private interface IResponseRule
		{
			IEnumerable<Violation> GetViolations(IOwinResponse response);
		}

		private class Violation
		{
			public string Message { get; set; }
			public Dictionary<string, HrefWrapper> Links { get; set; }
		}

		private struct HrefWrapper
		{
			public string href { get; set; }
		}
	}
}