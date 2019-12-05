namespace FluentlyHttpClient.GraphQL
{
	/// <summary>
	/// Request object for GraphQL requests.
	/// </summary>
	public class GqlQuery // deprecated: rename to GqlRequest or GqlRequestPayload?
	{
		/// <summary>
		/// Gets or sets GraphQL query.
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// Gets or sets GraphQL query variables.
		/// </summary>
		public object? Variables { get; set; }
	}
}