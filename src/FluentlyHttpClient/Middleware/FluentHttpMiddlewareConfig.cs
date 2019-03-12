using System;
using System.Diagnostics;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Middleware configuration.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpMiddlewareConfig
	{
		/// <summary>
		/// Debugger display.
		/// </summary>
		protected string DebuggerDisplay => $"Type: '{Type}', Args: '{Args}'";

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
		public FluentHttpMiddlewareConfig()
		{
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpMiddlewareConfig(Type type, object[] args = null)
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