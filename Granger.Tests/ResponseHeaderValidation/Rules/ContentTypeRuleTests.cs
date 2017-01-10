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
		protected override ContentTypeRule CreateRule() => new ContentTypeRule();

		protected override void Before()
		{
			Response.Headers["Content-Type"] = "application/json";
		}

		[Theory]
		[InlineData("Content-Type")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}
	}
}
