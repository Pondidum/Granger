using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class RequestHeaderValidatorMiddleware
	{
		private readonly AppFunc _next;

		public RequestHeaderValidatorMiddleware(AppFunc next)
		{
			_next = next;
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			var context = new OwinContext(environment);
			var request = context.Request;

			if (string.IsNullOrWhiteSpace(request.Accept))
			{
				var response = context.Response;
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				response.ContentType = "application/json";

				var json = JsonConvert.SerializeObject(new
				{
					Message = "The response was missing a recommend header: Accept"
				});

				await response.WriteAsync(json);
				return;
			}

			await _next.Invoke(environment);
		}
	}
}
