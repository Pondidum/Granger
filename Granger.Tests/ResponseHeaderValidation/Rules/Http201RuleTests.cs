using System.Linq;
using System.Net;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class Http201RuleTests : RuleTestFor<Http201Rule>
	{
		protected override IResponseRule CreateRule() => new Http201Rule();
		protected override void Before() => Response.StatusCode = (int)HttpStatusCode.Created;

		[Fact]
		public void When_there_is_no_location_header()
		{
			var violation = GetViolations().Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldEndWith("Location"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_there_is_a_location_header()
		{
			Response.Headers["Location"] = "http://example.com";

			GetViolations().ShouldBeEmpty();
		}
	}
}
