using System;
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
	public class ContractionMiddlewareTests : IDisposable
	{
		private Action<IOwinResponse> _respondWith;
		private readonly TestServer _server;

		public ContractionMiddlewareTests()
		{
			_server = TestServer.Create(app =>
			{
				app.Use<ContractionMiddleware>();

				app.Run(async context =>
				{
					_respondWith(context.Response);

					await Task.Yield();
				});
			});
		}

		private void JsonResponse(string json)
		{
			_respondWith = res =>
			{
				res.ContentType = "application/json";
				res.Write(json);
			};
		}

		[Fact]
		public async Task When_an_object_has_no_child_objects()
		{
			var json = JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30
			});

			JsonResponse(json);

			var response = await _server.CreateRequest("/resource").GetAsync();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(json);
		}


		[Fact]
		public async Task When_an_object_has_a_child_object_without_an_href()
		{
			JsonResponse(JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					line1 = "First",
					town = "Some Town",
					county = "Some County",
					country = "Some Country",
				}
			}));

			var response = await _server.CreateRequest("/resource").GetAsync();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					line1 = "First",
					town = "Some Town",
					county = "Some County",
					country = "Some Country",
				}
			}));
		}

		[Fact]
		public async Task When_an_object_has_a_child_object_with_an_href()
		{
			JsonResponse(JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					href = "http://example/1/address",
					line1 = "First",
					town = "Some Town",
					county = "Some County",
					country = "Some Country",
				}
			}));

			var response = await _server.CreateRequest("/resource").GetAsync();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					href = "http://example/1/address"
				}
			}));
		}

		public void Dispose()
		{
			_server.Dispose();
		}
	}
}