using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class UrlFuzzerMiddleware
	{
		private readonly AppFunc _next;

		public UrlFuzzerMiddleware(AppFunc next)
		{
			_next = next;
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			var context = new OwinContext(environment);

			await _next.Invoke(environment);
		}
	}
}
