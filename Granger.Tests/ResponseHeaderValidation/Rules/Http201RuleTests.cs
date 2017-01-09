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

		protected override void Before()
		{
			Response.StatusCode = (int)HttpStatusCode.Created;
			Response.Headers["Location"] = "http://example.com";
		}

		[Theory]
		[InlineData("Location")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}
	}
}
