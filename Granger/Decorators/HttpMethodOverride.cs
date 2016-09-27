using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Granger.Decorators
{
	public class HttpMethodOverride : OwinMiddleware
	{
		private static readonly string[] DefaultMethods = new[]
		{
			"get",
			"put",
			"patch",
			"delete",
			"head"
		};

		private HashSet<string> _allowedMethods;

		public HttpMethodOverride(OwinMiddleware next) : this(next, DefaultMethods)
		{
		}

		public HttpMethodOverride(OwinMiddleware next, IEnumerable<string> allowedMethods) : base(next)
		{
			_allowedMethods = new HashSet<string>(allowedMethods, StringComparer.OrdinalIgnoreCase);
		}

		public override async Task Invoke(IOwinContext context)
		{
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

			await Next.Invoke(context);
		}
	}
}
