﻿using System.Collections.Generic;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Interface which both <see cref="FluentHttpRequest"/> and <see cref="FluentHttpResponse"/> implements in order to share state across.
	/// This can be useful for adding extension methods on which would be beneficial for both request and response.
	/// </summary>
	public interface IFluentHttpMessageState : IFluentHttpMessageItems
	{
	}

	/// <summary>
	/// Interface which <see cref="FluentHttpRequestBuilder"/>, <see cref="FluentHttpRequest"/> and <see cref="FluentHttpResponse"/> implements. 
	/// This can be useful for adding extension methods across all of them.
	/// </summary>
	public interface  IFluentHttpMessageItems
	{
		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of request/response or middleware.
		/// </summary>
		IDictionary<object, object> Items { get; }
	}
}
