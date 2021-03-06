﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class CollectionRangeMiddleware : InterceptingMiddleware
	{
		public const int DefaultPageSize = 10;

		private readonly int _pageSize;

		// ReSharper disable once IntroduceOptionalParameters.Global
		// As this is an owin middleware, you cannot use optional paramters for the ctor
		public CollectionRangeMiddleware(AppFunc next) : this(next, DefaultPageSize)
		{
		}

		public CollectionRangeMiddleware(AppFunc next, int pageSize) : base(next)
		{
			_pageSize = pageSize;
		}

		protected override async Task<MemoryStream> AfterNext(IOwinContext context, MemoryStream internalMiddleware)
		{
			if (string.Equals(context.Response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase) == false)
				return await base.AfterNext(context, internalMiddleware);

			var json = Encoding.UTF8.GetString(internalMiddleware.ToArray());
			var jo = JToken.Parse(json);

			if (jo.Type == JTokenType.Array)
			{
				var start = GetOrDefault(context.Request, "start", 0);
				var limit = GetOrDefault(context.Request, "limit", _pageSize);

				var chopped = jo.Skip(start).Take(limit);

				jo = JToken.FromObject(chopped);
			}

			var bytes = Encoding.UTF8.GetBytes(jo.ToString(Formatting.None));

			return new MemoryStream(bytes);
		}

		private static int GetOrDefault(IOwinRequest request, string key, int defaultValue)
		{
			var param = request.Query.Get(key);

			int result;

			return int.TryParse(param, out result)
				? result
				: defaultValue;
		}
	}
}
