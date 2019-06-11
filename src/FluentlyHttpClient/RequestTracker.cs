using System.Collections.Generic;

namespace FluentlyHttpClient
{
	internal class FluentlyExecutionContext
	{
		public FluentHttpRequest Request { get; set; }
		public FluentHttpResponse Response { get; set; }
	}

	internal class RequestTracker
	{
		private readonly Dictionary<string, FluentlyExecutionContext> _contexts = new Dictionary<string, FluentlyExecutionContext>();

		public void Push(string key, FluentHttpRequest request)
		{
			if (_contexts.TryGetValue(key, out var context))
			{
				context.Request = request;
				return;
			}

			_contexts.Add(key, new FluentlyExecutionContext
			{
				Request = request
			});
		}

		public void Push(string key, FluentHttpResponse response)
		{
			if (_contexts.TryGetValue(key, out var context))
			{
				context.Response = response;
				return;
			}

			_contexts.Add(key, new FluentlyExecutionContext
			{
				Response = response
			});
		}

		public FluentlyExecutionContext Pop(string key)
		{
			if (!_contexts.TryGetValue(key, out var context))
				return null;

			_contexts.Remove(key);
			return context;
		}

		public bool TryPeek(string key, out FluentlyExecutionContext context)
			=> _contexts.TryGetValue(key, out context);
	}
}