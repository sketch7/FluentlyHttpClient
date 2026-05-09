using System.Collections.Concurrent;

namespace FluentlyHttpClient;

internal class FluentlyExecutionContext
{
	public FluentHttpRequest? Request { get; set; }
	public FluentHttpResponse Response { get; set; } = null!;
}

internal class RequestTracker
{
	private readonly ConcurrentDictionary<string, FluentlyExecutionContext> _contexts = new();

	public void Push(string key, FluentHttpRequest request)
	{
		var context = _contexts.GetOrAdd(key, _ => new());
		context.Request = request;
	}

	public void Push(string key, FluentHttpResponse response)
	{
		var context = _contexts.GetOrAdd(key, _ => new());
		context.Response = response;
	}

	public FluentlyExecutionContext Pop(string key)
	{
		_contexts.TryRemove(key, out var context);
		return context ?? throw new InvalidOperationException($"No execution context found for request ID '{key}'.");
	}

	public bool TryPeek(string key, out FluentlyExecutionContext? context)
		=> _contexts.TryGetValue(key, out context);
}