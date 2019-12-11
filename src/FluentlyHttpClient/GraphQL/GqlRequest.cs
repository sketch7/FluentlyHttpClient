using System;

namespace FluentlyHttpClient.GraphQL
{
	/// <summary>
	/// Request object for GraphQL requests.
	/// </summary>
	public class GqlRequest
	{
		/// <summary>
		/// Gets or sets the operation name.
		/// </summary>
		public string? OperationName { get; set; }

		/// <summary>
		/// Gets or sets GraphQL query.
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// Gets or sets GraphQL query variables.
		/// </summary>
		public object? Variables { get; set; }
	}

	[Obsolete("Use 'GqlRequest' instead.")]
	public class GqlQuery : GqlRequest
	{
	}
}