using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Granger.Decorators
{
	public class ContractionMiddleware : InterceptingMiddleware
	{
		public ContractionMiddleware(OwinMiddleware next) : base(next)
		{
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			var response = context.Response;

			if (string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return  await base.AfterNext(context, internalMiddleware);

			var content = Encoding.UTF8.GetString(internalMiddleware.ToArray());
			var jo = JObject.Parse(content);

			var toReplace = jo
				.Properties()
				.Where(token => token.Value.Type == JTokenType.Object)
				.Where(token => token.Value.Value<string>("href") != null)
				.ToList();

			foreach (var token in toReplace)
			{
				jo[token.Name] = JObject.FromObject(new
				{
					href = token.Value.Value<string>("href")
				});
			}

			return new MemoryStream(Encoding.UTF8.GetBytes(jo.ToString(Formatting.None)));
		}
	}
}
