using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class ResponseHeaderValidator
	{
		private Action<IOwinResponse> _configureResponse;
		private readonly TestServer _server;

		public ResponseHeaderValidator()
		{
			_configureResponse = response => { };
			_server = TestServer.Create(app =>
			{
				app.Run(async context =>
				{
					context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
					_configureResponse(context.Response);
					await Task.FromResult(0);
				});
			});
		}

		[Fact]
		public async Task When_the_response_has_no_content_type()
		{
			var response = await _server.CreateRequest("resource").GetAsync();
			var content = JToken.Parse(await response.Content.ReadAsStringAsync());

			response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
			content["Message"].ShouldBe("The response was missing a recommend header: Content-Type");
		}

		[Fact]
		public async Task When_the_response_has_a_content_type()
		{
			_configureResponse = res => res.Headers[HttpResponseHeader.ContentType.ToString()] = "application/json";

			var response = await _server.CreateRequest("resource").GetAsync();
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldBe("{}");
		}
	}
}
