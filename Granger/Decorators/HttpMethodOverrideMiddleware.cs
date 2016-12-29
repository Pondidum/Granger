using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class HttpMethodOverrideMiddleware
	{
		private static readonly string[] DefaultMethods = new[]
		{
			"get",
			"put",
			"patch",
			"delete",
			"head"
		};

		private readonly AppFunc _next;
		private readonly HashSet<string> _allowedMethods;

		public HttpMethodOverrideMiddleware(AppFunc next) : this(next, DefaultMethods)
		{
		}

		public HttpMethodOverrideMiddleware(AppFunc next, IEnumerable<string> allowedMethods)
		{
			_next = next;
			_allowedMethods = new HashSet<string>(allowedMethods, StringComparer.OrdinalIgnoreCase);
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			var context = new OwinContext(environment);

			if (context.Request.Method == HttpMethod.Post.Method)
			{
				var overrideMethod = context
					.Request
					.Query
					.GetValues("_method")
					.DefaultIfEmpty(HttpMethod.Post.Method)
					.First();

				if (_allowedMethods.Contains(overrideMethod) == false)
				{
					context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
					context.Response.Headers[HttpResponseHeader.Allow.ToString()] = string.Join(", ", _allowedMethods);
					return;
				}

				context.Request.Method = overrideMethod;

			}

			await _next.Invoke(environment);
		}
	}
}
