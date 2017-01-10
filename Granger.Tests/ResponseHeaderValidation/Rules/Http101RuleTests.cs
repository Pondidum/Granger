using System.Net;
using Granger.ResponseHeaderValidation;
using Granger.ResponseHeaderValidation.Rules;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class Http101RuleTests : RuleTestFor<Http101Rule>
	{
		protected override Http101Rule CreateRule() => new Http101Rule();
		protected override void Before()
		{
			Response.StatusCode = (int)HttpStatusCode.SwitchingProtocols;
			Response.Headers["Upgrade"] = "http/2.0";
		}

		[Theory]
		[InlineData("Upgrade")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}
	}
}