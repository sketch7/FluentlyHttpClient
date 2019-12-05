using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	internal class FluentMiddlewareHttpHandler : DelegatingHandler
	{
		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly FluentHttpClient _httpClient;
		private readonly RequestTracker _requestTracker;

		public FluentMiddlewareHttpHandler(
			IFluentHttpMiddlewareRunner middlewareRunner,
			FluentHttpClient httpClient,
			RequestTracker requestTracker,
			HttpMessageHandler? messageHandler = null
		) : base(messageHandler ?? new HttpClientHandler())
		{
			_middlewareRunner = middlewareRunner;
			_httpClient = httpClient;
			_requestTracker = requestTracker;
		}

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		)
		{
			FluentlyExecutionContext? context = null;
			var requestId = request.GetRequestId();
			if (requestId != null && !_requestTracker.TryPeek(requestId, out context))
				context = new FluentlyExecutionContext();

			var fluentlyRequest = context?.Request ?? request.ToFluentHttpRequest(_httpClient);

			var fluentlyResponse = await _middlewareRunner.Run(
				fluentlyRequest,
				async () =>
				{
					var response = await base.SendAsync(request, cancellationToken);
					return response.ToFluentHttpResponse(fluentlyRequest.Items);
				});

			if (context != null)
				_requestTracker.Push(requestId, fluentlyResponse);

			return fluentlyResponse?.Message;
		}
	}
}