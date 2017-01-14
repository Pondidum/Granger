using System;
using Newtonsoft.Json;

namespace Granger.ResponseHeaderValidation
{
	public struct HrefWrapper
	{
		[JsonProperty("href")]
		public Uri Href { get; set; }

		public static HrefWrapper From(string url)
		{
			return new HrefWrapper { Href = new Uri(url) };
		}

		public static HrefWrapper From(Uri url)
		{
			return new HrefWrapper { Href = url };
		}
	}
}
