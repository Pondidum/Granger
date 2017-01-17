using System.Net;
using Granger.ResponseHeaderValidation.Rules;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class Http206RuleTests : RuleTestForHeader<Http206Rule>
	{
		protected override Http206Rule CreateRule() => new Http206Rule();

		protected override void Before()
		{
			Response.StatusCode = (int)HttpStatusCode.PartialContent;
			Response.Headers["Content-Range"] = "bytes 0-14/28";
		}

		[Theory]
		[InlineData("Content-Range")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}
	}
}
