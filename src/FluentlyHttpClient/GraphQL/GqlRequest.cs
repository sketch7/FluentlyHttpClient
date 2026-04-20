namespace FluentlyHttpClient.GraphQL;

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
	public required string Query { get; set; }

	/// <summary>
	/// Gets or sets GraphQL query variables.
	/// </summary>
	public object? Variables { get; set; }
}

/// <summary>Obsolete GraphQL request object. Use <see cref="GqlRequest"/> instead.</summary>
[Obsolete("Use 'GqlRequest' instead.")]
public class GqlQuery : GqlRequest
{
}