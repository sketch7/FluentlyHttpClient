// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Request object for GraphQL requests
	/// </summary>
	public class GqlQuery
	{

		/// <summary>
		/// GraphQL query string
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// GraphQL object variables
		/// </summary>
		public object Variables { get; set; }
	}
}