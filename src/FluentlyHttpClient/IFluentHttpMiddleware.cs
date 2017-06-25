using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	public interface IFluentHttpMiddleware
	{
		Task<FluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}