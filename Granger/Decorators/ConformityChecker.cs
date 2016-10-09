using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Granger.Conformity;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;

namespace Granger.Decorators
{
	public class ConformityChecker : OwinMiddleware
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

		public override async Task Invoke(IOwinContext context)
		{
			await Next.Invoke(context);

			var request = context.Request;
			var headers = request.Headers.GetValues(HttpRequestHeader.ContentType.ToString());
			var contentType = headers?.FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);

			if (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return;

			var content = await context.Response.ReadAsString();
			var jo = JToken.Parse(content);

			var problems = _finder.Execute(jo).ToList();

			if (problems.Any())
			{
				var index = Math.Max(content.LastIndexOf('}'), content.LastIndexOf(']'));

				var before = content.Substring(0, index);
				var after = content.Substring(index);

				var output = "\"__conformity\":" + _renderer.Render(problems);

				if (jo.Type == JTokenType.Array)
					output = "{" + output + "}";

				await context.WriteString(string.Concat(
					before,
					before.EndsWith(",") ? "" : ",",
					output,
					after
				));
			}
			else
			{
				await context.WriteString(content);
			}
		}
	}
}
