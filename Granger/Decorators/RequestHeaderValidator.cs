using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Granger.Decorators
{
	public class RequestHeaderValidator : OwinMiddleware
	{
		public RequestHeaderValidator(OwinMiddleware next) : base(next)
		{
		}

		public override async Task Invoke(IOwinContext context)
		{
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

			await Next.Invoke(context);
		}
	}
}
