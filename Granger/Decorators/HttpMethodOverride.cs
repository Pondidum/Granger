using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class HttpMethodOverride : OwinMiddleware
	{
		public HttpMethodOverride(OwinMiddleware next) : base(next)
		{
		}

		public override Task Invoke(IOwinContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
