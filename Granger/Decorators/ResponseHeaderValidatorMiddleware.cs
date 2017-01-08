using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using Microsoft.Owin;
using Newtonsoft.Json;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Granger.Decorators
{
	public class ResponseHeaderValidatorMiddleware
	{
		private readonly AppFunc _next;
		private readonly List<IResponseRule> _rules;

		public ResponseHeaderValidatorMiddleware(AppFunc next)
		{
			_next = next;
			_rules = new List<IResponseRule>
			{
				new HttpRequiredHeadersRule(),
				new Http201Rule()
			};
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			await _next.Invoke(environment);

			var context = new OwinContext(environment);
			var response = context.Response;

			var violations = _rules.SelectMany(rule => rule.GetViolations(response)).ToArray();

			if (violations.Any())
			{
				response.StatusCode = (int)HttpStatusCode.InternalServerError;
				response.ContentType = "application/json";

				await response.WriteAsync(JsonConvert.SerializeObject(violations));
			}

			await Task.Yield();
		}
	}
}