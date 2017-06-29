using System;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Middleware options.
	/// </summary>
	public class MiddlewareOptions
	{
		/// <summary>
		/// Middleware type.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Middleware arguments.
		/// </summary>
		public object[] Args { get; set; }

		public MiddlewareOptions()
		{
		}

		public MiddlewareOptions(Type type, object[] args)
		{
			Type = type;
			Args = args;
		}

		public void Deconstruct(out Type type, out object[] args) { type = Type; args = Args; }
	}
}