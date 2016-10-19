﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Granger.Decorators;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

		private async Task<T> Get<T>(string path, HttpStatusCode expected = HttpStatusCode.OK)
		{
			var response = await _server
				.CreateRequest(path)
				.GetAsync();

			response.StatusCode.ShouldBe(expected);

			var content = await response
				.Content
				.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<T>(content);
		}

		[Fact]
		public async Task When_the_root_url_is_requested()
		{
			RootUrlHandler();

			var response = await Get<Root>("/");

			response.ShouldSatisfyAllConditions(
				() => response.Href.ShouldBe("http://localhost/"),
				() => response.FirstChild.Href.ShouldNotBe("http://localhost/children/1"),
				() => response.LastChild.Href.ShouldNotBe("http://localhost/children/5")
			);
		}

		[Fact]
		public async Task When_a_fuzzed_link_is_followed()
		{
			RootUrlHandler();
			ChildOneHandler();

			var root = await Get<Root>("/");
			var c1 = await Get<Child>(root.FirstChild.Href);

			c1.Href.ShouldNotBe("http://localhost/children/1");
		}

		[Fact]
		public async Task When_a_non_fuzzed_link_is_followed()
		{
			RootUrlHandler();
			ChildOneHandler();

			var root = await Get<Root>("/");
			var response = await Get<JToken>("/children/1", HttpStatusCode.NotFound);

			response.Value<string>("RequestedPath").ShouldBe("/children/1");
			response.Value<string>("SuggestedPath").ShouldBe(new Uri(root.FirstChild.Href).PathAndQuery.TrimStart('/'));
		}

		private void RootUrlHandler()
		{
			_handlers["/"] = async (req, res) =>
			{
				res.ContentType = "application/json";
				await res.WriteAsync(JsonConvert.SerializeObject(new
				{
					href = "http://localhost/",
					totalChildren = 5,
					firstChild = new { href = "http://localhost/children/1" },
					lastChild = new { href = "http://localhost/children/5" }
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
					href = "http://localhost/children/1",
					value = "first",
					collection = new { href = "http://localhost/children" },
					next = new { href = "http://localhost/children/2" }
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
					href = "http://localhost/children/2",
					value = "second",
					collection = new { href = "http://localhost/children" },
					previous = new { href = "http://localhost/children/1" },
					next = new { href = "http://localhost/children/3" }
				}));
			};
		}

		private class HrefWrapper
		{
			public string Href { get; set; }
		}

		private class Root
		{
			public string Href { get; set; }
			public HrefWrapper FirstChild { get; set; }
			public HrefWrapper LastChild { get; set; }
		}

		private class Child : HrefWrapper
		{
			public string Value { get; set; }
			public HrefWrapper Collection { get; set; }
			public HrefWrapper Previous { get; set; }
			public HrefWrapper Next { get; set; }
		}
	}
}
