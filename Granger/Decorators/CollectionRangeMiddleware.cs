using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Granger.Decorators
{
	public class CollectionRangeMiddleware : OwinMiddleware
	{
		public const int DefaultPageSize = 10;

		private readonly int _pageSize;

		// ReSharper disable once IntroduceOptionalParameters.Global
		// As this is an owin middleware, you cannot use optional paramters for the ctor
		public CollectionRangeMiddleware(OwinMiddleware next) : this(next, DefaultPageSize)
		{
		}

		public CollectionRangeMiddleware(OwinMiddleware next, int pageSize) : base(next)
		{
			_pageSize = pageSize;
		}

		public override async Task Invoke(IOwinContext context)
		{
			var stream = context.Response.Body;
			var buffer = new MemoryStream();

			context.Response.Body = buffer;

			await Next.Invoke(context);

			var contentType = context.Response.Headers.GetValues(HttpResponseHeader.ContentType.ToString());

			if (contentType != null && contentType.All(c => c.Equals("application/json", StringComparison.OrdinalIgnoreCase) == false))
			{
				buffer.Position = 0;
				await buffer.CopyToAsync(stream);
				return;
			}

			var json = Encoding.UTF8.GetString(buffer.ToArray());
			var jo = JToken.Parse(json);

			if (jo.Type == JTokenType.Array)
			{
				var start = GetOrDefault(context.Request, "start", 0);
				var limit = GetOrDefault(context.Request, "limit", _pageSize);

				var chopped = jo.Skip(start).Take(limit);

				jo = JToken.FromObject(chopped);
			}

			var bytes = Encoding.UTF8.GetBytes(jo.ToString(Formatting.None));

			await stream.WriteAsync(bytes, 0, bytes.Count());
		}

		private static int GetOrDefault(IOwinRequest request, string key, int defaultValue)
		{
			var param = request.Query.Get(key);

			int result;

			return int.TryParse(param, out result)
				? result
				: defaultValue;
		}


	}
}
