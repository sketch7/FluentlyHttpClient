using System.Threading.Tasks;

namespace FluentlyHttp
{
	public interface IFluentHttpMiddleware
	{
		Task<IFluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}