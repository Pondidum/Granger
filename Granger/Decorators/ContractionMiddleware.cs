using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class ContractionMiddleware : InterceptingMiddleware
	{
		private readonly string _keyField;

		public ContractionMiddleware(AppFunc next) : this(next, "href")
		{
		}

		public ContractionMiddleware(AppFunc next, string keyField) : base(next)
		{
			_keyField = keyField;
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			var response = context.Response;

			if (string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return  await base.AfterNext(context, internalMiddleware);

			var content = Encoding.UTF8.GetString(internalMiddleware.ToArray());
			var jo = JObject.Parse(content);

			var toReplace = Find(jo)
				.Where(token => token.Value.Type == JTokenType.Object)
				.Where(token => token.Value.Value<string>(_keyField) != null)
				.ToList();

			foreach (var token in toReplace)
			{
				token.Parent[token.Name] = JObject.FromObject(new Dictionary<string, string>
				{
					{ _keyField, token.Value.Value<string>(_keyField) }
				});
			}

			return new MemoryStream(Encoding.UTF8.GetBytes(jo.ToString(Formatting.None)));
		}


		private IEnumerable<JProperty> Find(JObject token)
		{
			foreach (var child in token.Properties())
			{
				yield return child;

				if (child.Value.Type != JTokenType.Object)
					continue;

				foreach (var inner in Find((JObject)child.Value))
					yield return inner;
			}
		}
	}
}
