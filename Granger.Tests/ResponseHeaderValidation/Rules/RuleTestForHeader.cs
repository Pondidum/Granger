using System.Linq;
using Granger.ResponseHeaderValidation;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public abstract class RuleTestForHeader<TRule> : RuleTestFor<TRule>
		where TRule : IResponseRule
	{
		protected void TestHeader(string header)
		{
			Before();

			Response.Headers[header] = null;

			var violation = Rule.GetViolations(Response).Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldEndWith(header),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		public abstract void When_testing_headers(string header);

		[Fact]
		public void When_headers_are_present()
		{
			Before();

			Rule.GetViolations(Response).ShouldBeEmpty();
		}
	}
}
