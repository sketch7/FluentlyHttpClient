using System;

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