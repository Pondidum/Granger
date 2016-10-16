using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Granger.Decorators;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class UrlFuzzerMiddlewareTests
	{
		private readonly TestServer _server;
		private readonly Dictionary<string, Func<IOwinRequest, IOwinResponse, Task>> _handlers;

		public UrlFuzzerMiddlewareTests()
		{
			_handlers = new Dictionary<string, Func<IOwinRequest, IOwinResponse, Task>>();
			_server = TestServer.Create(app =>
			{
				app.Use<UrlFuzzerMiddleware>();
				app.Run(async context =>
				{
					var request = context.Request;
					var response = context.Response;

					Func<IOwinRequest, IOwinResponse, Task> handler;

					if (_handlers.TryGetValue(request.Path.Value, out handler))
					{
						await handler(request, response);
						return;
					}

					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				});
			});
		}

		private async Task<T> Get<T>(string path)
		{
			var response = await _server
				.CreateRequest(path)
				.GetAsync();

			response.StatusCode.ShouldBe(HttpStatusCode.OK);

			var content = await response
				.Content
				.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<T>(content);
		}

		private async Task AttemptGet(string path)
		{
			var response = await _server
				.CreateRequest(path)
				.GetAsync();

			response.StatusCode.ShouldNotBe(HttpStatusCode.OK);
		}

		[Fact]
		public async Task When_the_root_url_is_requested()
		{
			RootUrlHandler();

			var response = await Get<Root>("/");

			response.ShouldSatisfyAllConditions(
				() => response.Href.ShouldBe("/"),
				() => response.FirstChild.Href.ShouldNotBe("/children/1"),
				() => response.LastChild.Href.ShouldNotBe("/children/5")
			);
		}

		[Fact]
		public async Task When_a_fuzzed_link_is_followed()
		{
			RootUrlHandler();
			ChildOneHandler();

			var root = await Get<Root>("/");
			var c1 = await Get<Child>(root.FirstChild.Href);

			c1.Href.ShouldNotBe("/children/1");
		}

		[Fact]
		public async Task When_a_non_fuzzed_link_is_followed()
		{
			RootUrlHandler();
			ChildOneHandler();

			await AttemptGet("/children/1");
		}

		private void RootUrlHandler()
		{
			_handlers["/"] = async (req, res) =>
			{
				res.ContentType = "application/json";
				await res.WriteAsync(JsonConvert.SerializeObject(new
				{
					href = "/",
					totalChildren = 5,
					firstChild = new { href = "/children/1" },
					lastChild = new { href = "/children/5" }
				}));
			};
		}

		private void ChildOneHandler()
		{
			_handlers["/children/1"] = async (req, res) =>
			{
				res.ContentType = "application/json";
				await res.WriteAsync(JsonConvert.SerializeObject(new
				{
					href = "/children/1",
					value = "first",
					collection = new { href = "/children" },
					next = new { href = "/children/2" }
				}));
			};
		}

		private void ChildTwoHandler()
		{
			_handlers["/children/2"] = async (req, res) =>
			{
				res.ContentType = "application/json";
				await res.WriteAsync(JsonConvert.SerializeObject(new
				{
					href = "/children/2",
					value = "second",
					collection = new { href = "/children" },
					previous = new { href = "/children/1" },
					next = new { href = "/children/3" }
				}));
			};
		}

		private interface IHref
		{
			string Href { get; set; }
		}

		private class Root : IHref
		{
			public string Href { get; set; }
			public IHref FirstChild { get; set; }
			public IHref LastChild { get; set; }
		}

		private class Child : IHref
		{
			public string Href { get; set; }
			public string Value { get; set; }
			public IHref Collection { get; set; }
			public IHref Previous { get; set; }
			public IHref Next { get; set; }
		}
	}
}
