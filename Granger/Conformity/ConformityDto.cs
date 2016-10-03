using System.Collections.Generic;
using System.Linq;

namespace Granger.Conformity
{
	public class ConformityDto
	{
		public IEnumerable<string> Paths { get; set; } = Enumerable.Empty<string>();
		public IEnumerable<object> Examples { get; set; } = Enumerable.Empty<object>();
	}
}
