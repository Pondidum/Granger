using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class ConformityChecker : OwinMiddleware
	{
		public ConformityChecker(OwinMiddleware next) : base(next)
		{
		}

		public override async Task Invoke(IOwinContext context)
		{
			await Next.Invoke(context);

			var request = context.Request;
			var headers = request.Headers.GetValues(HttpRequestHeader.ContentType.ToString());
			var contentType = headers?.FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);

			if (string.Equals(contentType, "application/json") == false)
				return;

		}
	}
}
