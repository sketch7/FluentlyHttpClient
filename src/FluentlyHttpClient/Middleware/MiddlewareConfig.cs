using System;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Middleware configuration.
	/// </summary>
	public class MiddlewareConfig
	{
		/// <summary>
		/// Gets or sets the type for the middleware.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets or sets the arguments for the middleware.
		/// </summary>
		public object[] Args { get; set; }

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public MiddlewareConfig()
		{
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public MiddlewareConfig(Type type, object[] args)
		{
			Type = type;
			Args = args;
		}

		/// <summary>
		/// Destructuring.
		/// </summary>
		public void Deconstruct(out Type type, out object[] args) { type = Type; args = Args; }
	}
}