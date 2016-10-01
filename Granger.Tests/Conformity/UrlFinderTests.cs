using System.Linq;
using Granger.Conformity;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Granger.Tests.Conformity
{
	public class UrlFinderTests
	{
		private readonly UrlFinder _finder;

		public UrlFinderTests()
		{
			_finder = new UrlFinder();
		}

		[Fact]
		public void When_given_a_blank_object()
		{
			_finder
				.Execute(JToken.FromObject(new {}))
				.ShouldBeEmpty();
		}

		[Fact]
		public void When_given_an_object_with_a_single_string_url()
		{
			_finder
				.Execute(JToken.FromObject(new { location = "http://example.com" }))
				.Single()
				.Path
				.ShouldBe("location");
		}

		[Fact]
		public void When_given_an_object_with_a_non_url_string()
		{
			_finder
				.Execute(JToken.FromObject(new { location = "something not a url" }))
				.ShouldBeEmpty();
		}

		[Fact]
		public void When_a_url_is_in_a_sub_object()
		{
			_finder
				.Execute(JToken.FromObject(new { inner = new { location = "http://exmaple.com" } }))
				.Single()
				.Path
				.ShouldBe("inner.location");
		}
	}
}
