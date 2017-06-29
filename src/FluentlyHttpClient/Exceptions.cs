using System;

/// <summary>
/// Thrown when validation for request fails.
/// </summary>
public class RequestValidationException : Exception
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public RequestValidationException(string message) : base(message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public RequestValidationException(string message, Exception inner) : base(message, inner)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
	}

	/// <summary>
	/// Initializes a new <see cref="RequestValidationException"/> for field which is missing.
	/// </summary>
	/// <param name="field">Missing field.</param>
	/// <returns></returns>
	public static RequestValidationException FieldNotSpecified(string field)
		=> new RequestValidationException($"{field} is not specified");
}

/// <summary>
/// Thrown when validation for client builder fails.
/// </summary>
public class ClientBuilderValidationException : Exception
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public ClientBuilderValidationException(string message) : base(message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public ClientBuilderValidationException(string message, Exception inner) : base(message, inner)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
	}

	/// <summary>
	/// Initializes a new <see cref="ClientBuilderValidationException"/> for field which is missing.
	/// </summary>
	/// <param name="field">Missing field.</param>
	/// <returns></returns>
	public static ClientBuilderValidationException FieldNotSpecified(string field)
		=> new ClientBuilderValidationException($"{field} is not specified");
}