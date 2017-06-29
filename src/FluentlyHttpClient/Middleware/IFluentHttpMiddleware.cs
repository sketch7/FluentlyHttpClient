using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Interface for HTTP middleware.
	/// </summary>
	public interface IFluentHttpMiddleware
	{
		/// <summary>
		/// Function to invoke when it runs.
		/// </summary>
		/// <param name="request">Request to send.</param>
		/// <returns>Returns response.</returns>
		Task<FluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}