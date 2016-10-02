﻿using System.Collections.Generic;
using System.Linq;
using Granger.Conformity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Granger.Tests.Conformity
{
	public class SuggestionRendererTests
	{
		private readonly SuggestionRenderer _renderer;

		public SuggestionRendererTests()
		{
			_renderer = new SuggestionRenderer();
		}

		[Fact]
		public void When_there_are_no_replacements()
		{
			var input = JToken.FromObject(new
			{
				location = "http://example.com"
			});

			var output = _renderer.Render(input, Enumerable.Empty<JToken>().ToList());

			output.ToString().ShouldBe(input.ToString());
		}

		[Fact]
		public void The_change_paths_are_reported()
		{
			var input = JToken.FromObject(new
			{
				location = "http://example.com",
				inner = new []
				{
					new { value = "http://other.example.com" }
				}
			});

			var replacements = new[]
			{
				input.SelectToken("location"),
				input.SelectToken("inner[0].value")
			};

			var output = _renderer.Render(input, replacements);

			var paths = output["__conformity"]["paths"].ToObject<IEnumerable<string>>();

			paths.ShouldBe(new[] { "location", "inner[0].value" });
		}

		[Fact]
		public void The_examples_are_based_off_the_first_violation()
		{
			var input = JToken.FromObject(new
			{
				location = "http://example.com",
				inner = new[]
				{
					new { value = "http://other.example.com" }
				}
			});

			var replacements = new[]
			{
				input.SelectToken("location"),
				input.SelectToken("inner[0].value")
			};

			var output = _renderer.Render(input, replacements);
			var examples = output["__conformity"]["examples"];

			var option1 = examples.First;
			var option2 = examples.Last;

			option1.ToString().ShouldBe(JToken.FromObject(new
			{
				location = new { href = "http://example.com" }
			}).ToString());

			option2.ToString().ShouldBe(JToken.FromObject(new
			{
				href = "http://example.com"
			}).ToString());
		}
	}
}
