using System.Collections.Generic;
using Microsoft.Owin;

namespace Granger.ResponseHeaderValidation
{
	public interface IResponseRule
	{
		IEnumerable<Violation> GetViolations(IOwinResponse response);
	}
}
