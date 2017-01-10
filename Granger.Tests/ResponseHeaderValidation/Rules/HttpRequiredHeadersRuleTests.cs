using System;
using System.Collections.Generic;
using System.Linq;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using Microsoft.Owin;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class HttpRequiredHeadersRuleTests : RuleTestFor<HttpRequiredHeadersRule>
	{
		protected override HttpRequiredHeadersRule CreateRule() => new HttpRequiredHeadersRule();

		protected override void Before()
		{
			Response.Headers["Content-Type"] = "application/json";
			Response.Headers["Content-Length"] = "23";
		}

		[Theory]
		[InlineData("Content-Type")]
		[InlineData("Content-Length")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}
	}
}