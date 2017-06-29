using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Run specified middleware, and finally send the given request.
		/// </summary>
		/// <param name="middlewareCollection">Middleware to pipe.</param>
		/// <param name="request">Request to send.</param>
		/// <param name="send">Actual send function.</param>
		/// <returns>Returns response.</returns>
		Task<FluentHttpResponse> Run(IList<MiddlewareOptions> middlewareCollection, FluentHttpRequest request, FluentHttpRequestDelegate send);
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

		public async Task<FluentHttpResponse> Run(IList<MiddlewareOptions> middlewareCollection, FluentHttpRequest request, FluentHttpRequestDelegate send)
		{
			if (middlewareCollection.Count == 0)
				return await send(request);

			FluentHttpResponse httpResult = null;
			IFluentHttpMiddleware previousMiddleware = null;

			for (int i = middlewareCollection.Count; i-- > 0;)
			{
				request.CancellationToken.ThrowIfCancellationRequested();
				var middlewareOptions = middlewareCollection[i];

				var isLast = middlewareCollection.Count - 1 == i;
				var isFirst = i == 0;
				var next = isLast
					? send
					: previousMiddleware.Invoke;

				object[] ctor;
				if (middlewareOptions.Args == null)
					ctor = new object[] { next };
				else
				{
					ctor = new object[middlewareOptions.Args.Length + 1];
					ctor[0] = next;
					Array.Copy(middlewareOptions.Args, 0, ctor, 1, middlewareOptions.Args.Length);
				}

				var instance = (IFluentHttpMiddleware)ActivatorUtilities.CreateInstance(_serviceProvider, middlewareOptions.Type, ctor);

				if (isFirst)
					httpResult = await instance.Invoke(request);
				else
					previousMiddleware = instance;
			}
			return httpResult;
		}

	}
}