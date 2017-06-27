using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	public interface IFluentHttpMiddleware
	{
		Task<FluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}