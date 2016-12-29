using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Granger.Conformity;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class ConformityCheckerMiddleware : InterceptingMiddleware
	{
		private readonly UrlFinder _finder;
		private readonly SuggestionRenderer _renderer;

		public ConformityCheckerMiddleware(AppFunc next)
			: this(next, null, null)
		{
		}

		public ConformityCheckerMiddleware(AppFunc next, UrlFinder finder, SuggestionRenderer renderer) : base(next)
		{
			_finder = finder ?? new UrlFinder();
			_renderer = renderer ?? new SuggestionRenderer();
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			var request = context.Request;

			if (string.Equals(context.Response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return await base.AfterNext(context, internalMiddleware);

			var content = Encoding.UTF8.GetString(internalMiddleware.ToArray());
			var jo = JToken.Parse(content);

			var problems = _finder.Execute(jo).ToList();

			if (problems.Any() == false)
				return await base.AfterNext(context, internalMiddleware);

			var index = Math.Max(content.LastIndexOf('}'), content.LastIndexOf(']'));

			var before = content.Substring(0, index);
			var after = content.Substring(index);

			var output = "\"__conformity\":" + _renderer.Render(problems);

			if (jo.Type == JTokenType.Array)
				output = "{" + output + "}";

			return new MemoryStream(Encoding.UTF8.GetBytes(string.Concat(
				before,
				before.EndsWith(",") ? "" : ",",
				output,
				after
			)));
		}
	}
}
