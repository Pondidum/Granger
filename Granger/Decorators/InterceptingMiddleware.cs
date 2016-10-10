using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class InterceptingMiddleware : OwinMiddleware
	{
		public InterceptingMiddleware(OwinMiddleware next) : base(next)
		{
		}

		public override async Task Invoke(IOwinContext context)
		{
			var stream = context.Response.Body;
			var buffer = new MemoryStream();

			context.Response.Body = buffer;

			await Next.Invoke(context);

			var intercepted = await AfterNext(context, buffer);
			intercepted.Position = 0;

			await intercepted.CopyToAsync(stream);
		}

		protected virtual async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			await Task.Yield();
			return internalMiddleware;
		}
	}
}
