using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Granger.Conformity
{
	public class SuggestionRenderer
	{
		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public JToken Render(JToken original, ICollection<JToken> toChange)
		{
			if (toChange.Any() == false)
				return original;

			var report = new ConformityDto
			{
				Paths = toChange.Select(change => change.Path),
				Examples = new object[]
				{
					new { location = new { href = toChange.First().ToString() } },
					new { href = toChange.First().ToString() }
				}
			};

			var result = original.DeepClone();
			result["__conformity"] = JToken.FromObject(report, JsonSerializer.Create(JsonSettings));

			return result;
		}

		private class ConformityDto
		{
			public IEnumerable<string> Paths { get; set; }
			public IEnumerable<object> Examples { get; set; }
		}
	}
}
