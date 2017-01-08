using System.Linq;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class ContentTypeRuleTests : RuleTestFor<ContentTypeRule>
	{
		protected override IResponseRule CreateRule() => new ContentTypeRule();

		[Fact]
		public void When_there_is_no_content_type()
		{
			var violation = GetViolations().Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldEndWith("Content-Type"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_there_is_a_content_type()
		{
			Response.ContentType.Returns("application/json");

			GetViolations().ShouldBeEmpty();
		}
	}
}
