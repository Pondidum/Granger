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

			if (string.IsNullOrWhiteSpace(context.Response.ContentType))
			{
				var response = context.Response;

				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				response.ContentType = "application/json";

				var json = JsonConvert.SerializeObject(new
				{
					Message = "The response was missing a recommend header: Content-Type"
				});

				await response.WriteAsync(json);
			}

			await Task.Yield();
		}
	}
}
