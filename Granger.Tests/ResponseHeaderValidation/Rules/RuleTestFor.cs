using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Granger.ResponseHeaderValidation;
using Microsoft.Owin;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public abstract class RuleTestFor<TRule>
		where TRule : IResponseRule
	{
		private readonly IResponseRule _rule;
		protected IOwinResponse Response { get; }

		protected RuleTestFor()
		{
			_rule = CreateRule();
			Response = Substitute.For<IOwinResponse>();
			Response.Headers.Returns(new HeaderDictionary(new Dictionary<string, string[]>()));

			Response.ContentType = Arg.Do<string>(val => Response.Headers["Content-Type"] = val);
			Response.ContentType.Returns(ci => Response.Headers["Content-Type"]);

			Response.ContentLength = Arg.Do<int>(val => Response.Headers["Content-Type"] = Convert.ToString(val));
			Response.ContentLength.Returns(ci => int.Parse(Response.Headers["Content-Type"]));
		}

		protected abstract IResponseRule CreateRule();

		protected virtual void Before()
		{
		}

		protected void TestHeader(string header)
		{
			Before();

			Response.Headers[header] = null;

			var violation = _rule.GetViolations(Response).Single();

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

			_rule.GetViolations(Response).ShouldBeEmpty();
		}
	}
}
