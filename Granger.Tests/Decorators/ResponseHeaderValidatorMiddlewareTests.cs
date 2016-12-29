using System;
using System.Net;
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

		public ResponseHeaderValidatorMiddlewareTests()
		{
			_configureResponse = response => { };
			_server = TestServer.Create(app =>
			{
				app.UseResponseHeaderValidator();
				app.Run(async context =>
				{
					_configureResponse(context.Response);
					await Task.Yield();
				});
			});
		}

		[Fact]
		public async Task When_the_response_has_no_content_type()
		{
			var response = await _server.CreateRequest("resource").GetAsync();
			var json = await response.Content.ReadAsStringAsync();
			var content = JToken.Parse(json);

			response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
			content["Message"].ShouldBe("The response was missing a recommend header: Content-Type");
		}

		[Fact]
		public async Task When_the_response_has_a_content_type()
		{
			_configureResponse = res => res.ContentType = "application/json";

			var response = await _server.CreateRequest("resource").GetAsync();
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldBe("");
		}
	}
}
