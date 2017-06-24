using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	public interface IFluentHttpMiddleware
	{
		Task<IFluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}