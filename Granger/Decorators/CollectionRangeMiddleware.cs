using System;
using System.IO;
using System.Linq;
using System.Net;
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
			await Next.Invoke(context);

			var contentType = context.Response.Headers.GetValues(HttpResponseHeader.ContentType.ToString());

			if (contentType != null && contentType.All(c => c.Equals("application/json", StringComparison.OrdinalIgnoreCase) == false))
				return;

			var jo = ReadJson(context.Response);

			if (jo.Type == JTokenType.Array)
			{
				var start = GetOrDefault(context.Request, "start", 0);
				var limit = GetOrDefault(context.Request, "limit", _pageSize);

				var chopped = jo.Skip(start).Take(limit);

				jo = JToken.FromObject(chopped); ;
			}

			context.Response.Body = WriteJson(jo);
		}

		private static MemoryStream WriteJson(JToken jo)
		{
			var ms = new MemoryStream();
			var streamWriter = new StreamWriter(ms);
			var writer = new JsonTextWriter(streamWriter);

			jo.WriteTo(writer);
			writer.Flush();
			ms.Position = 0;

			return ms;
		}

		private static JToken ReadJson(IOwinResponse response)
		{
			using (var streamReader = new StreamReader(response.Body))
			using (var jsonReader = new JsonTextReader(streamReader))
				return JToken.ReadFrom(jsonReader);
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
