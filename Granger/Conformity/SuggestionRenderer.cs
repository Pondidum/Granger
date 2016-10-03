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

		public virtual JToken Render(ICollection<JToken> toChange)
		{
			if (toChange.Any() == false)
				return JToken.FromObject(new ConformityDto(), JsonSerializer.Create(JsonSettings));

			var report = new ConformityDto
			{
				Paths = toChange.Select(change => change.Path),
				Examples = new object[]
				{
					new { location = new { href = toChange.First().ToString() } },
					new { href = toChange.First().ToString() }
				}
			};

			return JToken.FromObject(report, JsonSerializer.Create(JsonSettings));
		}
	}
}
