using System;
using System.IO;
using Granger.ResponseHeaderValidation.Rules;
using Shouldly;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

namespace Granger.Tests.ResponseHeaderValidation.Rules
{
	public class Http206RuleTests : RuleTestForHeader<Http206Rule>
	{
		protected override Http206Rule CreateRule() => new Http206Rule();

		protected override void Before()
		{
			Response.StatusCode = (int)HttpStatusCode.PartialContent;
			//Response.Headers["Content-Range"] = "bytes 0-14/28";
		}

		[Theory]
		[InlineData("Content-Range")]
		public override void When_testing_headers(string header)
		{
			TestHeader(header);
		}

		[Fact]
		public void When_single_part()
		{
			Before();
			Response.ContentType = "image/png";
			Response.ContentLength = 16;
			Response.Headers["content-range"] = "bytes 4-12/16";

			Rule.GetViolations(Response).ShouldBeEmpty();
		}

		[Fact]
		public void When_multi_part()
		{
			Before();

			var boundary = Guid.NewGuid().ToString();
			var sourceContent = Encoding.UTF8.GetBytes("This is a test, and not a png");

			var multi = new MultipartContent("byteranges", boundary);

			var i = 0;
			while (i < sourceContent.Length)
			{
				var sub = sourceContent.Skip(i).Take(4).ToArray();
				var range = new RangeHeaderValue(i, i + sub.Length);

				multi.Add(new ByteRangeStreamContent(
					new MemoryStream(sourceContent),
					range,
					"image/png"));

				i += sub.Length;
			}

			var content = new MemoryStream();
			multi.CopyToAsync(content).Wait();

			Response.ContentType = multi.Headers.ContentType.MediaType;
			Response.ContentLength = multi.Headers.ContentLength.Value;
			Response.Body = content;

			Rule.GetViolations(Response).ShouldBeEmpty();
		}

		//* if single part, content-range, content-length, content-type
		//* if multi part, content-length, content-type: multipart/byteranges; boundary=qwedqwf, NO content-range
		//* parts must have content-range, should have content-type
	}
}
