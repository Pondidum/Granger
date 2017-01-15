using System.IO;
using Granger.ResponseHeaderValidation.Rules;
using System.Linq;
using System.Net;
using System.Text;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class Http205RuleTests : RuleTestFor<Http205Rule>
	{
		protected override Http205Rule CreateRule() => new Http205Rule();
		protected override void Before()
		{
			Response.StatusCode = (int)HttpStatusCode.NoContent;
		}

		[Fact]
		public void When_called_with_a_different_statuscode()
		{
			Before();
			Response.StatusCode++;

			Rule.GetViolations(Response).ShouldBeEmpty();
		}

		[Fact]
		public void When_there_is_content_length_set()
		{
			Before();
			Response.ContentLength = 53;

			var violation = Rule.GetViolations(Response).Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldBe("The Content-Length header must not be set when returning 205: Reset Content"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_there_is_content()
		{
			Before();
			Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("testing!")) { Position = 0 };

			var violation = Rule.GetViolations(Response).Single();

			violation.ShouldSatisfyAllConditions(
				() => violation.Message.ShouldBe("There must not be any content when returning 205: Reset Content"),
				() => violation.Links.ShouldContainKey("rfc")
			);
		}

		[Fact]
		public void When_there_is_no_content()
		{
			Before();
			Rule.GetViolations(Response).ShouldBeEmpty();
		}
	}
}
