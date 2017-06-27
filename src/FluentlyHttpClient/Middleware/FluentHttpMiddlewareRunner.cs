using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Http middleware runner (executor).
	/// </summary>
	public interface IFluentHttpMiddlewareRunner
	{
		/// <summary>
		/// Run specified middlewares, and finally send the given request.
		/// </summary>
		/// <param name="middleware">Middleware to pipe.</param>
		/// <param name="request">Request to send.</param>
		/// <param name="send">Actual send function.</param>
		/// <returns>Returns response.</returns>
		Task<FluentHttpResponse> Run(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send);
	}

	/// <summary>
	/// Http middleware runner default implementation.
	/// </summary>
	public class FluentHttpMiddlewareRunner : IFluentHttpMiddlewareRunner
	{
		private readonly IServiceProvider _serviceProvider;

		public FluentHttpMiddlewareRunner(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task<FluentHttpResponse> Run(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send)
		{
			if (middleware.Count == 0)
				return await send(request);

			FluentHttpResponse httpResult = null;
			IFluentHttpMiddleware previousMiddleware = null;

			for (int i = middleware.Count; i-- > 0;)
			{
				var type = middleware[i];

				var isLast = middleware.Count - 1 == i;
				var isFirst = i == 0;
				var next = isLast
					? send
					: previousMiddleware.Invoke;
				var instance = (IFluentHttpMiddleware)ActivatorUtilities.CreateInstance(_serviceProvider, type, next);

				if (isFirst)
					httpResult = await instance.Invoke(request);
				else
					previousMiddleware = instance;
			}
			return httpResult;
		}

	}
}