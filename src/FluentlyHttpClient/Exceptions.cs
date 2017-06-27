using System;

/// <summary>
/// Thrown when validation for request fails.
/// </summary>
public class RequestValidationException : Exception
{
	public RequestValidationException(string message) : base(message)
	{
	}

	public RequestValidationException(string message, Exception inner) : base(message, inner)
	{
	}

	public static RequestValidationException FieldNotSpecified(string field)
		=> new RequestValidationException($"{field} is not specified");
}

/// <summary>
/// Thrown when validation for client builder fails.
/// </summary>
public class ClientBuilderValidationException : Exception
{
	public ClientBuilderValidationException(string message) : base(message)
	{
	}

	public ClientBuilderValidationException(string message, Exception inner) : base(message, inner)
	{
	}

	public static ClientBuilderValidationException FieldNotSpecified(string field)
		=> new ClientBuilderValidationException($"{field} is not specified");
}