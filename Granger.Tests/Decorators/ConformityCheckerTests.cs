using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Granger.Conformity;
using Granger.Decorators;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class ConformityCheckerTests : IDisposable
	{
		private Action<IOwinResponse> _controllerResponse;
		private readonly TestServer _server;
		private readonly UrlFinder _finder;
		private readonly SuggestionRenderer _renderer;

		public ConformityCheckerTests()
		{
			_finder = Substitute.For<UrlFinder>();
			_renderer = Substitute.For<SuggestionRenderer>();

			_server = TestServer.Create(app =>
			{
				app.Use<ConformityChecker>(_finder, _renderer);
				app.Run(async context =>
				{
					_controllerResponse(context.Response);
					await Task.Yield();
				});
			});
		}


		private void JsonResponse(object toSerialize)
		{
			JsonResponse(JsonConvert.SerializeObject(toSerialize));
		}

		private void JsonResponse(string json)
		{
			_controllerResponse = response =>
			{
				response.ContentType = "application/json";
				response.Write(json);
			};
		}

		private async Task<HttpResponseMessage> Execute(string contentType = "application/json")
		{
			return await _server
				.CreateRequest("/path")
				.AddHeader(HttpRequestHeader.ContentType.ToString(), contentType)
				.GetAsync();
		}

		private static string FromStream(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return reader.ReadToEnd();
		}

		[Fact]
		public async Task When_the_response_is_not_json()
		{
			var xml = "<root>http://example.com</root>";

			_controllerResponse = res =>
			{
				res.ContentType = "text/xml";
				res.Write(xml);
			};

			var response = await Execute("text/xml");

			var content = await response.Content.ReadAsStringAsync();
			content.ShouldBe(xml);
		}

		[Fact]
		public async Task When_the_response_is_json_with_nothing_to_check_for()
		{
			var json = "{ \"name\":\"Andy Dote\" }";

			_finder.Execute(Arg.Any<JToken>()).Returns(Enumerable.Empty<JToken>());

			JsonResponse(json);

			var response = await Execute();

			var content = await response.Content.ReadAsStringAsync();
			content.ShouldBe(json);
		}

		[Fact]
		public async Task When_the_response_has_a_conforming_href()
		{
			_finder.Execute(Arg.Any<JToken>()).Returns(Enumerable.Empty<JToken>());
			var json = JsonConvert.SerializeObject(new { href = "http://test.com" });

			JsonResponse(json);

			var response = await Execute();

			var content = await response.Content.ReadAsStringAsync();
			content.ShouldBe(json);
		}

		[Fact]
		public async Task When_the_response_has_non_conforming_properties()
		{
			var problems = new[]
			{
				JToken.FromObject(new { })
			};

			_finder.Execute(Arg.Any<JToken>()).Returns(problems);
			_renderer.Render(Arg.Any<ICollection<JToken>>()).Returns(JToken.Parse("{}"));

			var json = JsonConvert.SerializeObject(new { href = "http://test.com" });

			JsonResponse(json);

			var response = await Execute();

			_renderer.Received().Render(Arg.Any<ICollection<JToken>>());
		}

		[Fact]
		public async Task When_the_response_is_an_array()
		{
			var problems = new[]
			{
				JToken.FromObject(new { })
			};

			_finder.Execute(Arg.Any<JToken>()).Returns(problems);
			_renderer.Render(Arg.Any<ICollection<JToken>>()).Returns(JToken.Parse("{}"));

			var json = JsonConvert.SerializeObject(new object[]
			{
				new { name = "andy" },
				new { name = "dave" }
			});

			JsonResponse(json);

			var response = await Execute();

			var expected = JsonConvert.SerializeObject(new object[]
			{
				new { name = "andy" },
				new { name = "dave" },
				new { __conformity = new {} }
			});

			var content = await response.Content.ReadAsStringAsync();
			content.ShouldBe(expected);
		}

		private static string Json(object obj) => JsonConvert.SerializeObject(obj);

		public void Dispose()
		{
			_server?.Dispose();
		}
	}
}
