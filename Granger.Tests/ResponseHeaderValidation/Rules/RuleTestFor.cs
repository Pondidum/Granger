using System;
using System.Collections.Generic;
using Granger.ResponseHeaderValidation;
using Microsoft.Owin;
using NSubstitute;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public abstract class RuleTestFor<TRule>
		where TRule : IResponseRule
	{
		protected TRule Rule { get; }
		protected IOwinResponse Response { get; }

		protected RuleTestFor()
		{
			Rule = CreateRule();
			Response = Substitute.For<IOwinResponse>();
			Response.Headers.Returns(new HeaderDictionary(new Dictionary<string, string[]>()));

			Response.ContentType = Arg.Do<string>(val => Response.Headers["Content-Type"] = val);
			Response.ContentType.Returns(ci => Response.Headers["Content-Type"]);

			Response.ContentLength = Arg.Do<int>(val => Response.Headers["Content-Type"] = Convert.ToString(val));
			Response.ContentLength.Returns(ci => int.Parse(Response.Headers["Content-Type"]));
		}

		protected abstract TRule CreateRule();

		protected virtual void Before()
		{
		}
	}
}
