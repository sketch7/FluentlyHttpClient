using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Middleware pipeline builder
	/// </summary>
	public class FluentHttpMiddlewareBuilder
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<FluentHttpMiddlewareConfig> _middleware = new List<FluentHttpMiddlewareConfig>();

		/// <summary>
		/// Gets the middleware count.
		/// </summary>
		public int Count => _middleware.Count;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="serviceProvider"></param>
		public FluentHttpMiddlewareBuilder(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Add pipe middleware, they will execute according to the order they are registered.
		/// </summary>
		/// <param name="args">Additional arguments to be used within the pipe ctor.</param>
		public FluentHttpMiddlewareBuilder Add<T>(params object[] args)
			where T : IFluentHttpMiddleware
			=> Add(typeof(T), args);

		/// <summary>
		/// Add pipe middleware, they will execute according to the order they are registered.
		/// </summary>
		/// <param name="type">Pipe type which must implements <see cref="IFluentHttpMiddleware"/>.</param>
		/// <param name="args">Additional arguments to be used within the pipe ctor.</param>
		public FluentHttpMiddlewareBuilder Add(Type type, params object[] args)
		{
			if (!typeof(IFluentHttpMiddleware).IsAssignableFrom(type))
				throw new ArgumentException($"Type '{type.FullName}' must implement {nameof(IFluentHttpMiddleware)}.", nameof(type));

			_middleware.Add(new FluentHttpMiddlewareConfig(type, args));
			return this;
		}

		/// <summary>
		/// Adds a collection of middleware configs.
		/// </summary>
		/// <param name="middleware">Middleware configs to add.</param>
		public FluentHttpMiddlewareBuilder AddRange(IEnumerable<FluentHttpMiddlewareConfig> middleware)
		{
			_middleware.AddRange(middleware);
			return this;
		}

		/// <summary>
		/// Get all middleware configs.
		/// </summary>
		public IEnumerable<FluentHttpMiddlewareConfig> GetAll() => _middleware.AsReadOnly();

		/// <summary>
		/// Build configured <see cref="IFluentHttpMiddlewareRunner"/>.
		/// </summary>
		/// <param name="httpClient">HTTP client which consumes this.</param>
		public IFluentHttpMiddlewareRunner Build(IFluentHttpClient httpClient)
		{
			var middleware = _middleware.ToList();
			middleware.Add(new FluentHttpMiddlewareConfig(typeof(ActionExecuteMiddleware)));

			var clientContext = new FluentHttpMiddlewareClientContext(httpClient.Identifier);

			IFluentHttpMiddleware previous = null;
			for (int i = middleware.Count; i-- > 0;)
			{
				var pipe = middleware[i];
				var isLast = middleware.Count - 1 == i;
				var isFirst = i == 0;

				object[] ctor;
				if (!isLast)
				{
					FluentHttpMiddlewareDelegate next = previous.Invoke;
					if (pipe.Args == null)
						ctor = new object[] { next, clientContext };
					else
					{
						const int additionalCtorArgs = 2;
						ctor = new object[pipe.Args.Length + additionalCtorArgs];
						ctor[0] = next;
						ctor[1] = clientContext;
						Array.Copy(pipe.Args, 0, ctor, additionalCtorArgs, pipe.Args.Length);
					}
				} else
					ctor = new object[] { };
				var instance = (IFluentHttpMiddleware)ActivatorUtilities.CreateInstance(_serviceProvider, pipe.Type, ctor);

				if (isFirst)
					return new FluentHttpMiddlewareRunner(instance);
				previous = instance;
			}
			throw new InvalidOperationException("Middleware wasn't build correctly.");
		}
	}


	/// <summary>
	/// Action invoker pipe, which actually triggers the users defined function. Generally invoked as the last pipe.
	/// </summary>
	internal class ActionExecuteMiddleware : IFluentHttpMiddleware
	{
		public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context) => await context.Func();
	}

}