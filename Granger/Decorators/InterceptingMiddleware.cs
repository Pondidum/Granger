using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class InterceptingMiddleware
	{
		private readonly AppFunc _next;

		public InterceptingMiddleware(AppFunc next)
		{
			_next = next;
		}

		public async Task Invoke(IDictionary<string, object> env )
		{
			var context = new OwinContext(env);

			var control = await BeforeNext(context);

			if (control == MiddlewareChain.Stop)
				return;

			var stream = context.Response.Body;
			var buffer = new MemoryStream();

			context.Response.Body = buffer;

			await _next.Invoke(env);

			var intercepted = await AfterNext(context, buffer);
			intercepted.Position = 0;

			await intercepted.CopyToAsync(stream);
		}

		protected virtual async Task<MiddlewareChain> BeforeNext(IOwinContext context)
		{
			await Task.Yield();
			return MiddlewareChain.Continue;
		}

		protected virtual async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			await Task.Yield();
			return internalMiddleware;
		}
	}

	public enum MiddlewareChain
	{
		Continue,
		Stop
	}
}
