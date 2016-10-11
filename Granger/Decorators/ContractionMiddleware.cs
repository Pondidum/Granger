using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class ContractionMiddleware : InterceptingMiddleware
	{
		public ContractionMiddleware(OwinMiddleware next) : base(next)
		{
		}

		//public override async Task Invoke(IOwinContext context)
		//{
		//	await Next.Invoke(context);

		//	var response = context.Response;

		//	if (string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
		//		return;

		//	var jo = response.ReadJson();

		//	context.WriteJson(jo);
		//}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			var response = context.Response;

			if (string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return  await base.AfterNext(context, internalMiddleware);

			return internalMiddleware;
		}
	}
}
