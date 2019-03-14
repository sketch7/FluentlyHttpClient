﻿using System.Net.Http;
using FluentlyHttpClient;
using FluentlyHttpClient.Caching;

namespace FluentHttpClient.Entity
{
	public class HttpResponse : IMessageItemStore
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Hash { get; set; }
		public string Url { get; set; }
		public string Content { get; set; }
		public FluentHttpHeaders Headers { get; set; }
		public int StatusCode { get; set; }
		public string ReasonPhrase { get; set; }
		public string Version { get; set; }
		public FluentHttpHeaders ContentHeaders { get; set; }
		public HttpRequestMessage RequestMessage { get; set; }
	}
}