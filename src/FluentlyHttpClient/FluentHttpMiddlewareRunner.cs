using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentlyHttp
{
	public interface IFluentHttpMiddlewareRunner
	{
		Task<IFluentHttpResponse> Run<T>(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send);
	}

	public class FluentHttpMiddlewareRunner : IFluentHttpMiddlewareRunner
	{
		private readonly IServiceProvider _serviceProvider;

		public FluentHttpMiddlewareRunner(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task<IFluentHttpResponse> Run<T>(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send)
		{
			if (middleware.Count == 0)
				return await send(request);

			IFluentHttpResponse httpResult = null;
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