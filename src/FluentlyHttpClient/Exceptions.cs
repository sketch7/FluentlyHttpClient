namespace FluentlyHttpClient;

/// <summary>
/// Thrown when validation for request fails.
/// </summary>
public class RequestValidationException : Exception
{
	/// <summary>Initializes a new instance with the specified message.</summary>
	public RequestValidationException(string message) : base(message)
	{
	}

	/// <summary>Initializes a new instance with the specified message and inner exception.</summary>
	public RequestValidationException(string message, Exception inner) : base(message, inner)
	{
	}

	/// <summary>
	/// Initializes a new instance.
	/// </summary>
	public RequestValidationException() : base()
	{
	}

	/// <summary>
	/// Initializes a new <see cref="RequestValidationException"/> for field which is missing.
	/// </summary>
	/// <param name="field">Missing field.</param>
	/// <returns></returns>
	public static RequestValidationException FieldNotSpecified(string field)
		=> new($"{field} is not specified");
}

/// <summary>
/// Thrown when validation for client builder fails.
/// </summary>
public class ClientBuilderValidationException : Exception
{
	/// <summary>Initializes a new instance with the specified message.</summary>
	public ClientBuilderValidationException(string message) : base(message)
	{
	}

	/// <summary>Initializes a new instance with the specified message and inner exception.</summary>
	public ClientBuilderValidationException(string message, Exception inner) : base(message, inner)
	{
	}

	/// <summary>
	/// Initializes a new instance.
	/// </summary>
	public ClientBuilderValidationException() : base()
	{
	}

	/// <summary>
	/// Initializes a new <see cref="ClientBuilderValidationException"/> for field which is missing.
	/// </summary>
	/// <param name="field">Missing field.</param>
	/// <returns></returns>
	public static ClientBuilderValidationException FieldNotSpecified(string field)
		=> new($"{field} is not specified");
}