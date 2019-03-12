using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Interface for fluent HTTP middleware.
	/// </summary>
	public interface IFluentHttpMiddleware
	{
		/// <summary>
		/// Function to invoke when middleware runs.
		/// </summary>
		/// <param name="context">Middleware invoke context.</param>
		/// <returns>Returns response.</returns>
		Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context);
	}
}