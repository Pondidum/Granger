using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Granger.Decorators
{
	public class ResponseHeaderValidator : OwinMiddleware
	{
		public ResponseHeaderValidator(OwinMiddleware next) : base(next)
		{
		}

		public override async Task Invoke(IOwinContext context)
		{
			await Next.Invoke(context);

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
