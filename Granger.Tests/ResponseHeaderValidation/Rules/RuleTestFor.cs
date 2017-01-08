using System.Collections.Generic;
using System.Linq;
using Granger.ResponseHeaderValidation;
using Microsoft.Owin;
using NSubstitute;

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
		}

		protected abstract IResponseRule CreateRule();

		protected virtual void Before()
		{
		}

		protected IEnumerable<Violation> GetViolations()
		{
			Before();
			return _rule.GetViolations(Response).ToArray();
		}
	}
}
