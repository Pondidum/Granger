using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class ResponseHeaderValidatorMiddlewareTests
	{
		private Action<IOwinResponse> _configureResponse;
		private readonly TestServer _server;

		private JToken _content;
		private HttpResponseMessage _response;

		public ResponseHeaderValidatorMiddlewareTests()
		{
			_configureResponse = response => { };
			_server = TestServer.Create(app =>
			{
				app.UseResponseHeaderValidator();
				app.Run(async context =>
				{
					context.Response.ContentLength = 0;
					context.Response.ContentType = "application/json";

					_configureResponse(context.Response);

					await Task.Yield();
				});
			});
		}

		private async Task GetResponse()
		{
			var response = await _server.CreateRequest("resource").GetAsync();
			var json = await response.Content.ReadAsStringAsync();

			_content = JArray.Parse(json).First;
			_response = response;

		}

		private async Task CheckFor(string header)
		{
			_configureResponse = res => res.Headers[header] = null;

			await GetResponse();

			_response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

			_content["Message"].ShouldBe($"The response was missing a recommend header: {header}");
			_content["Links"]["rfc"]["href"].ShouldNotBeNull();
		}

		[Fact]
		public async Task When_the_response_has_all_headers()
		{
			var response = await _server.CreateRequest("resource").GetAsync();
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldBe("");
		}

		[Fact]
		public async Task When_missing_content_type_header() => await CheckFor("Content-Type");

		[Fact]
		public async Task When_missing_the_content_length_header() => await CheckFor("Content-Length");

		[Fact]
		public async Task When_the_response_is_201_and_has_a_location_header()
		{
			_configureResponse = res =>
			{
				res.ContentType = "application/json";
				res.StatusCode = (int)HttpStatusCode.Created;
				res.Headers["Location"] = "http://example.com/some/resource/1";
			};

			var response = await _server.CreateRequest("resource").GetAsync();
			var json = await response.Content.ReadAsStringAsync();

			response.StatusCode.ShouldBe(HttpStatusCode.Created);
			json.ShouldBe("");
		}

		[Fact]
		public async Task When_the_response_is_201_and_has_no_location_header()
		{
			_configureResponse = res =>
			{
				res.ContentType = "application/json";
				res.StatusCode = (int)HttpStatusCode.Created;
			};

			var response = await _server.CreateRequest("resource").GetAsync();
			var json = await response.Content.ReadAsStringAsync();
			var content = JArray.Parse(json).First;

			response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
			content["Message"].ShouldBe("The response was missing a recommend header: Location");
		}
	}
}
