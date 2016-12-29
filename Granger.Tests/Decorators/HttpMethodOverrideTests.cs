using System;
using System.Net;
using System.Threading.Tasks;
using Granger.Decorators;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class HttpMethodOverrideTests : IDisposable
	{
		private TestServer _server;
		private IOwinRequest _request;

		private void CreateServer()
		{
			_server = TestServer.Create(app =>
			{
				app.UseHttpMethodOverride();
				app.Run(async context =>
				{
					_request = context.Request;
					await Task.FromResult(0);
				});
			});
		}

		private void CreateServerWithWhitelist(params string[] methods)
		{
			_server = TestServer.Create(app =>
			{
				app.Use<HttpMethodOverrideMiddleware>(new object[] { methods });

				app.Run(async context =>
				{
					_request = context.Request;
					await Task.FromResult(0);
				});
			});
		}

		[Fact]
		public async Task When_the_route_has_no_override()
		{
			CreateServer();

			var response = await _server.HttpClient.GetAsync("resource");

			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			_request.Method.ShouldBe("GET");
		}

		[Theory]
		[InlineData("get")]
		[InlineData("put")]
		[InlineData("patch")]
		[InlineData("delete")]
		[InlineData("head")]
		public async Task When_the_route_has_an_override_and_is_not_a_post(string method)
		{
			CreateServer();

			var response = await _server.CreateRequest("resource?_method=DELETE").SendAsync(method);

			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			_request.Method.ShouldBe(method);
		}

		[Theory]
		[InlineData("get")]
		[InlineData("put")]
		[InlineData("patch")]
		[InlineData("delete")]
		[InlineData("head")]
		public async Task When_the_route_has_an_override_and_is_a_post(string method)
		{
			CreateServer();

			var response = await _server.CreateRequest("resource?_method=" + method).SendAsync("POST");

			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			_request.Method.ShouldBe(method);
		}

		[Theory]
		[InlineData("get")]
		[InlineData("put")]
		[InlineData("head")]
		public async Task When_using_a_restricted_method_list_and_the_supplied_method_is_not_whitelisted(string method)
		{
			CreateServerWithWhitelist("DELETE", "PATCH");

			var response = await _server.CreateRequest("resource?_method=" + method).SendAsync("POST");

			response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);

			_request.ShouldBeNull();
		}

		[Theory]
		[InlineData("patch")]
		[InlineData("delete")]
		public async Task When_using_a_restricted_method_list_and_the_supplied_method_is_whitelisted(string method)
		{
			CreateServerWithWhitelist("DELETE", "PATCH");

			var response = await _server.CreateRequest("resource?_method=" + method).SendAsync("POST");

			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			_request.Method.ShouldBe(method);
		}

		public void Dispose()
		{
			_server.Dispose();
		}
	}
}
