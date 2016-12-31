using System;
using Newtonsoft.Json;

namespace Granger.ResponseHeaderValidation
{
	public struct HrefWrapper
	{
		[JsonProperty("href")]
		public Uri Href { get; set; }
	}
}
