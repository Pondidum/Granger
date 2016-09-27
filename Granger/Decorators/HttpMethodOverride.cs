using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class HttpMethodOverride : OwinMiddleware
	{
		public HttpMethodOverride(OwinMiddleware next) : base(next)
		{
		}

		public override async Task Invoke(IOwinContext context)
		{
			if (context.Request.Method == HttpMethod.Post.Method)
				context.Request.Method = context
					.Request
					.Query
					.GetValues("_method")
					.DefaultIfEmpty(HttpMethod.Post.Method)
					.First();

			await Next.Invoke(context);
		}
	}
}
