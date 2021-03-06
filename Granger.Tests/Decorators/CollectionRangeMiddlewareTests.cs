﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
	public class CollectionRangeMiddlewareTests : IDisposable
	{
		private TestServer _server;

		private void CreateServer(Action<IOwinResponse> handle)
		{
			_server = TestServer.Create(app =>
			{
				app.UseCollectionRange();
				app.Run(async context =>
				{
					handle(context.Response);
					await Task.Yield();
				});
			});
		}

		private void CreateServerWithPageSize(int pageSize, Action<IOwinResponse> handle)
		{
			_server = TestServer.Create(app =>
			{
				app.UseCollectionRange(pageSize);
				app.Run(async context =>
				{
					handle(context.Response);
					await Task.Yield();
				});
			});
		}


		[Fact]
		public async Task When_the_response_is_not_json()
		{
			var xml = "<collection><item /><item /><item /></collection>";

			CreateServer(res =>
			{
				res.ContentType = "text/xml";
				res.Write(xml);
			});

			var response = await _server.CreateRequest("resource?start=0&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(xml);
		}

		[Fact]
		public async Task When_the_response_is_json_but_not_a_collection()
		{
			var json = JsonConvert.SerializeObject(new { name = "Andy", age = 30 });

			CreateServer(res => ReturnJson(res, json));

			var response = await _server.CreateRequest("resource?start=0&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(json);
		}

		[Fact]
		public async Task When_the_response_is_a_collection_but_smaller_than_a_page()
		{
			var json = JsonConvert.SerializeObject(Enumerable.Range(0, 7));

			CreateServer(res => ReturnJson(res, json));

			var response = await _server.CreateRequest("resource?start=0&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(json);
		}

		[Fact]
		public async Task When_no_parameters_are_specified()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServer(res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(collection.Take(10)));
		}

		[Fact]
		public async Task When_the_start_is_negative()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServer(res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource?start=-40&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(collection.Take(10)));
		}

		[Fact]
		public async Task When_the_limit_is_negative()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServer(res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource?start=0&limit=-5").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(Enumerable.Empty<int>()));
		}

		[Fact]
		public async Task When_a_page_which_exists_is_requested()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServer(res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource?start=5&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(collection.Skip(5).Take(10)));
		}

		[Fact]
		public async Task When_a_page_which_doesnt_exist_is_requested()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServer(res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource?start=50&limit=10").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(Enumerable.Empty<int>()));
		}

		[Fact]
		public async Task When_a_custom_page_size_is_used()
		{
			var collection = Enumerable.Range(0, 20);

			CreateServerWithPageSize(7, res => ReturnJson(res, JsonConvert.SerializeObject(collection)));

			var response = await _server.CreateRequest("resource?start=5").GetAsync();

			StringFromStream(await response.Content.ReadAsStreamAsync())
				.ShouldBe(JsonConvert.SerializeObject(collection.Skip(5).Take(7)));
		}

		private static void ReturnJson(IOwinResponse res, string json)
		{
			res.ContentType = "application/json";
			res.Write(json);
		}

		private static string StringFromStream(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return reader.ReadToEnd();
		}

		public void Dispose()
		{
			_server?.Dispose();
		}
	}
}
