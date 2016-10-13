using System;
using System.Threading.Tasks;
using Granger.Decorators;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.Decorators
{
	public class ContractionMiddlewareTests
	{
		private async Task<string> JsonResponseWithId(string key, string json)
		{
			return await JsonResponse(json, key);
		}

		private async Task<string> JsonResponse(string json, string key = "href")
		{
			Action<IAppBuilder> host = app =>
			{
				app.Use<ContractionMiddleware>(key);

				app.Run(async context =>
				{
					context.Response.ContentType = "application/json";
					context.Response.Write(json);

					await Task.Yield();
				});
			};

			using (var server = TestServer.Create(host))
			{
				var response = await server.CreateRequest("/resource").GetAsync();
				return await response.Content.ReadAsStringAsync();
			}
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

			var response = await JsonResponse(json);

			response.ShouldBe(json);
		}

		[Fact]
		public async Task When_an_object_has_a_child_object_without_an_href()
		{
			var response = await JsonResponse(JsonConvert.SerializeObject(new
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

			response.ShouldBe(JsonConvert.SerializeObject(new
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
			var response = await JsonResponse(JsonConvert.SerializeObject(new
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
				},
				email = new
				{
					value = "test@home.com",
					type = "home"
				}
			}));

			response.ShouldBe(JsonConvert.SerializeObject(new
			{
				href = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					href = "http://example/1/address"
				},
				email = new
				{
					value = "test@home.com",
					type = "home"
				}
			}));
		}

		[Fact]
		public async Task When_an_object_has_a_grand_child_object_with_an_href()
		{
			var response = await JsonResponse(JsonConvert.SerializeObject(new
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
					postcode = new
					{
						href = "http://example/1/address/postcode",
						code = "testing",
						value = 123
					}
				}
			}));

			response.ShouldBe(JsonConvert.SerializeObject(new
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
					postcode = new
					{
						href = "http://example/1/address/postcode"
					}
				}
			}));
		}

		[Fact]
		public async Task When_an_object_has_a_child_and_grand_child_object_with_an_href()
		{
			var response = await JsonResponse(JsonConvert.SerializeObject(new
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
					postcode = new
					{
						href = "http://example/1/address/postcode",
						code = "testing",
						value = 123
					}
				}
			}));

			response.ShouldBe(JsonConvert.SerializeObject(new
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

		[Fact]
		public async Task When_an_object_has_a_child_object_with_a_custom_id()
		{
			var response = await JsonResponseWithId("id", JsonConvert.SerializeObject(new
			{
				id = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					id = "http://example/1/address",
					line1 = "First",
					town = "Some Town",
					county = "Some County",
					country = "Some Country",
				},
				email = new
				{
					value = "test@home.com",
					type = "home"
				}
			}));

			response.ShouldBe(JsonConvert.SerializeObject(new
			{
				id = "http://example/1",
				name = "andy dote",
				age = 30,
				address = new
				{
					id = "http://example/1/address"
				},
				email = new
				{
					value = "test@home.com",
					type = "home"
				}
			}));
		}
	}
}
