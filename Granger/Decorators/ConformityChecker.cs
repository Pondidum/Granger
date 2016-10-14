using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Granger.Conformity;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;

namespace Granger.Decorators
{
	public class ConformityChecker : InterceptingMiddleware
	{
		private readonly UrlFinder _finder;
		private readonly SuggestionRenderer _renderer;

		public ConformityChecker(OwinMiddleware next)
			: this(next, new UrlFinder(), new SuggestionRenderer())
		{
		}

		public ConformityChecker(OwinMiddleware next, UrlFinder finder, SuggestionRenderer renderer) : base(next)
		{
			_finder = finder;
			_renderer = renderer;
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			var request = context.Request;

			if (string.Equals(request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
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
