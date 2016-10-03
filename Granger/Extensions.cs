using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Granger
{
	public static class Extensions
	{
		public static void WriteJson(this IOwinContext context, JToken jo)
		{
			var ms = new MemoryStream();
			var streamWriter = new StreamWriter(ms);
			var writer = new JsonTextWriter(streamWriter);

			jo.WriteTo(writer);
			writer.Flush();
			ms.Position = 0;

			context.Response.Body = ms;
		}

		public static async Task WriteString(this IOwinContext context, string input)
		{
			var ms = new MemoryStream();
			var writer = new StreamWriter(ms);

			await writer.WriteAsync(input);
			await writer.FlushAsync();

			ms.Position = 0;

			context.Response.Body = ms;
		}

		public static JToken ReadJson(this IOwinResponse response)
		{
			using (var streamReader = new StreamReader(response.Body))
			using (var jsonReader = new JsonTextReader(streamReader))
				return JToken.ReadFrom(jsonReader);
		}

		public static async Task<string> ReadAsString(this IOwinResponse response)
		{
			using (var reader = new StreamReader(response.Body))
				return await reader.ReadToEndAsync();
		}
	}
}
