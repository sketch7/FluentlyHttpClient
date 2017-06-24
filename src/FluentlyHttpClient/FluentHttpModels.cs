using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentlyHttp
{
	public delegate Task<IFluentHttpResponse> FluentHttpRequestDelegate(FluentHttpRequest request);
	
	public class FluentHttpRequest
	{
		public HttpRequestMessage RawRequest { get; }

		public FluentHttpRequest(HttpRequestMessage rawRequest)
		{
			RawRequest = rawRequest;
		}

		public HttpMethod Method => RawRequest.Method;
		public Uri Url => RawRequest.RequestUri;
		// todo: remove?
		public object Data { get; set; }
	}

	public interface IFluentHttpResponse
	{
		HttpStatusCode StatusCode { get; }
		bool IsSuccessStatusCode { get; }
		void EnsureSuccessStatusCode();
		string ReasonPhrase { get; }
		HttpResponseHeaders Headers { get; }
		IDictionary<object, object> Items { get; set; }
	}

	public class FluentHttpResponse<T> : IFluentHttpResponse
	{
		public HttpResponseMessage RawResponse { get; }

		public FluentHttpResponse(HttpResponseMessage rawResponse)
		{
			RawResponse = rawResponse;
		}

		public T Data { get; set; }

		public HttpStatusCode StatusCode => RawResponse.StatusCode;
		public bool IsSuccessStatusCode => RawResponse.IsSuccessStatusCode;
		public void EnsureSuccessStatusCode() => RawResponse.EnsureSuccessStatusCode();
		public string ReasonPhrase => RawResponse.ReasonPhrase;
		public HttpResponseHeaders Headers => RawResponse.Headers;
		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of request/response.
		/// </summary>
		public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
	}

	
}
