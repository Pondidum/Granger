using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Granger.Decorators;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Tests.Decorators
{
	public class InterceptingMiddlewareTests : IDisposable
	{
		private TestServer _server;
		private Action<IOwinRequest, IOwinResponse> _innerHandle;
		private Func<IOwinContext, MemoryStream, Task<MemoryStream>> _postHandle;

		private async Task<HttpResponseMessage> Get()
		{
			_server = TestServer.Create(app =>
			{
				app.Use<TestPostInterceptMiddleware>(_postHandle);
				app.Run(async context =>
				{
					_innerHandle(context.Request, context.Response);
					await Task.Yield();
				});
			});

			return await _server.HttpClient.GetAsync("/resource");
		}

		[Fact]
		public async Task When_the_post_intercept_does_nothing()
		{
			var content = "This is the content";
			_innerHandle = (req, res) => res.Write(content);

			var response = await Get();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(content);
		}

		[Fact]
		public async Task When_the_post_intercept_returns_the_stream_at_the_beginning()
		{
			var content = "This is the content";
			_innerHandle = (req, res) => res.Write(content);
			_postHandle = async (context, stream) =>
			{
				await Task.Yield();
				stream.Position = 0;
				return stream;
			};

			var response = await Get();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(content);
		}

		[Fact]
		public async Task When_the_post_intercept_returns_the_stream_at_the_end()
		{
			var content = "This is the content";
			_innerHandle = (req, res) => res.Write(content);
			_postHandle = async (context, stream) =>
			{
				await Task.Yield();
				stream.Position = stream.Length;
				return stream;
			};

			var response = await Get();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(content);
		}

		[Fact]
		public async Task When_the_interceptor_replaces_the_stream()
		{
			var replacement = "This is the replacement";

			_innerHandle = (req, res) => res.Write("This is the content");
			_postHandle = async (context, stream) =>
			{
				await Task.Yield();
				return new MemoryStream(Encoding.UTF8.GetBytes(replacement));
			};

			var response = await Get();
			var responseContent = await response.Content.ReadAsStringAsync();

			responseContent.ShouldBe(replacement);
		}

		public void Dispose()
		{
			_server.Dispose();
		}

		private class TestPostInterceptMiddleware : InterceptingMiddleware
		{
			private readonly Func<IOwinContext, MemoryStream, Task<MemoryStream>> _handle;

			public TestPostInterceptMiddleware(AppFunc next, Func<IOwinContext, MemoryStream, Task<MemoryStream>> handle) : base(next)
			{
				_handle = handle;
			}

			protected override Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
			{
				if (_handle == null)
					return base.AfterNext(context, internalMiddleware);
				else
					return _handle(context, internalMiddleware);
			}
		}
	}
}
