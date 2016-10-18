using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class UrlFuzzerMiddleware : InterceptingMiddleware
	{
		private static readonly Regex UrlExpression = new Regex(@"""href""\s*?:\s?""(.*?)""");

		private readonly ConcurrentDictionary<string, string> _urlMap;

		public UrlFuzzerMiddleware(AppFunc next) : base(next)
		{
			_urlMap = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_urlMap["/"] = "/";
		}

		protected override async Task<MiddlewareChain> BeforeNext(IOwinContext context)
		{
			var path = context.Request.Path.Value;
			var searchPath = path == "/" ? "/" : path.TrimStart('/');

			var newPath = _urlMap.Where(pair => pair.Value.Equals(searchPath, StringComparison.OrdinalIgnoreCase)).ToArray();

			if (newPath.Any() == false)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return MiddlewareChain.Stop;
			}

			context.Request.Path = new PathString(newPath.First().Key);

			return MiddlewareChain.Continue;
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			if (string.Equals(context.Response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return await base.AfterNext(context, internalMiddleware);

			var json = Encoding.UTF8.GetString(internalMiddleware.ToArray());

			var result = UrlExpression.Replace(json, match =>
			{
				var url = new Uri(match.Groups[1].Value);
				var builder = new UriBuilder(url);

				var newPath = _urlMap.GetOrAdd(builder.Path, key => Guid.NewGuid().ToString());
				builder.Path = newPath;

				return $"\"href\":\"{builder.Uri}\"";
			});

			return new MemoryStream(Encoding.UTF8.GetBytes(result));
		}
	}
}
