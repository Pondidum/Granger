using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Granger.Decorators;
using Newtonsoft.Json.Linq;

namespace Granger.Conformity
{
	public class UrlFinder
	{
		public virtual IEnumerable<JToken> Execute(JToken token)
		{
			var rx = new Regex("^https?://");

			return Find(token, t => t.Type == JTokenType.String)
				.Where(t => rx.IsMatch(t.ToString()))
				.Where(t => t.Path.Split('.').Last() != "href");
		}

		private IEnumerable<JToken> Find(JToken token, Func<JToken, bool> condition)
		{
			foreach (var child in token.Children())
			{
				if (condition(child))
					yield return child;

				foreach (var inner in Find(child, condition))
					yield return inner;
			}
		}
	}
}
