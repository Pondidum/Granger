using System.IO;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Granger
{
	public static class Extensions
	{
		public static MemoryStream WriteJson(this JToken jo)
		{
			var ms = new MemoryStream();
			var streamWriter = new StreamWriter(ms);
			var writer = new JsonTextWriter(streamWriter);

			jo.WriteTo(writer);
			writer.Flush();
			ms.Position = 0;

			return ms;
		}

		public static JToken ReadJson(this IOwinResponse response)
		{
			using (var streamReader = new StreamReader(response.Body))
			using (var jsonReader = new JsonTextReader(streamReader))
				return JToken.ReadFrom(jsonReader);
		}
	}
}
