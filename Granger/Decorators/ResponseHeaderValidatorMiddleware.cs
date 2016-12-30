using System.Collections.Generic;
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

		public ResponseHeaderValidatorMiddleware(AppFunc next)
		{
			_next = next;
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			await _next.Invoke(environment);

			var context = new OwinContext(environment);
			var response = context.Response;

			if (string.IsNullOrWhiteSpace(response.ContentType))
			{
				var json = JsonConvert.SerializeObject(new
				{
					Message = "The response was missing a recommend header: Content-Type",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", new HrefWrapper { href = "https://tools.ietf.org/html/rfc2616#section-7.2.1" } }
					}
				});

				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				response.ContentType = "application/json";

				await response.WriteAsync(json);
				return;
			}

			if (response.StatusCode == (int)HttpStatusCode.Created && response.Headers.ContainsKey("Location") == false)
			{
				var json = JsonConvert.SerializeObject(new
				{
					Message = "The response was missing a recommend header: Location",
					Links = new Dictionary<string, HrefWrapper>
					{
						{ "rfc", new HrefWrapper { href = "https://tools.ietf.org/html/rfc7231#section-6.3.2" } }
					}
				});

				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				response.ContentType = "application/json";

				await response.WriteAsync(json);
				return;
			}

			await Task.Yield();
		}

		private struct HrefWrapper
		{
			public string href { get; set; }
		}
	}
}