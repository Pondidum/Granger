using System.Collections.Generic;
using System.Linq;
using Granger.Conformity;
using Granger.Decorators;
using Owin;

namespace Granger
{
	public static class Extensions
	{
		public static IAppBuilder UseUrlFuzzer(this IAppBuilder appBuilder, IEnumerable<string> whitelist = null)
		{
			return appBuilder.Use<UrlFuzzerMiddleware>(whitelist ?? Enumerable.Empty<string>());
		}

		public static IAppBuilder UseResponseHeaderValidator(this IAppBuilder appBuilder)
		{
			return appBuilder.Use<ResponseHeaderValidatorMiddleware>();
		}

		public static IAppBuilder UseRequestHeaderValidator(this IAppBuilder appBuilder)
		{
			return appBuilder.Use<RequestHeaderValidatorMiddleware>();
		}

		public static IAppBuilder UseHttpMethodOverride(this IAppBuilder appBuilder, IEnumerable<string> allowedMethods = null)
		{
			return allowedMethods == null
				? appBuilder.Use<HttpMethodOverrideMiddleware>()
				: appBuilder.Use<HttpMethodOverrideMiddleware>(allowedMethods);
		}

		public static IAppBuilder UseContractionMiddleware(this IAppBuilder appBuilder, string key = null)
		{
			return key == null
				? appBuilder.Use<ContractionMiddleware>()
				: appBuilder.Use<ContractionMiddleware>(key);
		}

		public static IAppBuilder UseConformityChecker(this IAppBuilder appBuilder, UrlFinder finder = null, SuggestionRenderer renderer = null)
		{
			return appBuilder.Use<ConformityCheckerMiddleware>(finder, renderer);
		}

		public static IAppBuilder UseCollectionRange(this IAppBuilder appBuilder, int? defaultPageSize = null)
		{
			return defaultPageSize.HasValue
				? appBuilder.Use<CollectionRangeMiddleware>(defaultPageSize)
				: appBuilder.Use<CollectionRangeMiddleware>();
		}

	}
}
