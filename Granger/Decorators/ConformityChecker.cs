using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class ConformityChecker : OwinMiddleware
	{
		public ConformityChecker(OwinMiddleware next) : base(next)
		{
		}

		public override Task Invoke(IOwinContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
