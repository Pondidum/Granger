using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class RequestHeaderValidatorTests
	{
		private readonly TestServer _server;

		public RequestHeaderValidatorTests()
		{
			_server = TestServer.Create(app =>
			{
				app.UseRequestHeaderValidator();
				app.Run(async context =>
				{
					await Task.Yield();
				});
			});
		}

		[Fact]
		public async Task When_the_request_has_no_accept()
		{
			var response = await _server
				.CreateRequest("resource")
				.GetAsync();
			var content = JToken.Parse(await response.Content.ReadAsStringAsync());

			response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
			content["Message"].ShouldBe("The response was missing a recommend header: Accept");
		}

		[Fact]
		public async Task When_the_request_has_a_accept()
		{
			var response = await _server
				.CreateRequest("resource")
				.AddHeader("Accept", "application/json")
				.GetAsync();
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldBe("");
		}
	}
}
