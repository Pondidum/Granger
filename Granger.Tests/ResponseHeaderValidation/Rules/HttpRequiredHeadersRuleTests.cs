using System.Linq;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class HttpRequiredHeadersRuleTests : RuleTestFor<HttpRequiredHeadersRule>
	{
		protected override IResponseRule CreateRule() => new HttpRequiredHeadersRule();

		[Fact]
		public void When_there_is_no_content_length()
		{
			Response.Headers["Content-Type"] = "application/json";

			var violation = GetViolations().Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldEndWith("Content-Length"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_there_is_no_content_type()
		{
			Response.Headers["Content-Length"] = "23";

			var violation = GetViolations().Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldEndWith("Content-Type"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_both_headers_are_present()
		{
			Response.Headers["Content-Type"] = "application/json";
			Response.Headers["Content-Length"] = "23";

			GetViolations().ShouldBeEmpty();
		}

		[Fact]
		public void When_neither_headers_are_present()
		{
			GetViolations().Count().ShouldBe(2);
		}
	}
}