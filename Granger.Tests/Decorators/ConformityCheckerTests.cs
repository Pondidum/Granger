using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class ConformityCheckerTests : IDisposable
	{
		private TestServer _server;
		private IOwinResponse _response;

		private void CreateServer(Action<IOwinResponse> handle)
		{
			_server = TestServer.Create(app =>
			{
				app.Run(async context =>
				{
					handle(context.Response);
					_response = context.Response;
					await Task.Yield();
				});
			});
		}

		private static void Content(IOwinResponse res, string contentType, string content)
		{
			res.Headers[HttpResponseHeader.ContentType.ToString()] = contentType;
			res.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		private void Execute()
		{
			_server.CreateRequest("/path").GetAsync().Wait();
		}

		private static string FromStream(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return reader.ReadToEnd();
		}

		[Fact]
		public void When_the_response_is_not_json()
		{
			var xml = "<root>http://example.com</root>";

			CreateServer(res => Content(res, "text/xml", xml));

			Execute();

			FromStream(_response.Body).ShouldBe(xml);
		}

		[Fact]
		public void When_the_response_is_json_with_nothing_to_check_for()
		{
			var json = JsonConvert.SerializeObject(new { Name = "Andy" });

			CreateServer(res => Content(res, "application/json", json));

			Execute();

			FromStream(_response.Body).ShouldBe(json);
		}

		[Fact]
		public void When_the_response_has_a_conforming_href()
		{
			var json = JsonConvert.SerializeObject(new { href= "http://example.com" });

			CreateServer(res => Content(res, "application/json", json));

			Execute();

			FromStream(_response.Body).ShouldBe(json);
		}

		[Fact]
		public void When_the_response_has_a_non_conforming_href()
		{
			var dto = new { location = "http://example.com" };

			CreateServer(res => Content(res, "application/json", JsonConvert.SerializeObject(dto)));

			Execute();

			var expectedResponse = new
			{
				Original = dto,
				Options = new object[]
				{
					new { href = "http://example.com" },
					new { location = new { href = "http://example.com" } }
				}
			};

			FromStream(_response.Body).ShouldBe(Json(expectedResponse));
		}



		private static string Json(object obj) => JsonConvert.SerializeObject(obj);

		public void Dispose()
		{
			_server?.Dispose();
		}
	}
}
