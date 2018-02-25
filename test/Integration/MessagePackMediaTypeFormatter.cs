using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MessagePack;

namespace FluentlyHttpClient.Test.Integration
{
	public class MessagePackMediaTypeFormatter : MediaTypeFormatter
	{
		private const string MediaType = "application/x-msgpack";
		private readonly IFormatterResolver _resolver;

		public MessagePackMediaTypeFormatter()
			: this(null)
		{
		}

		public MessagePackMediaTypeFormatter(IFormatterResolver resolver)
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(MediaType));
			_resolver = resolver ?? MessagePackSerializer.DefaultResolver;
		}

		public override bool CanReadType(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			return IsAllowedType(type);
		}

		public override bool CanWriteType(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			return IsAllowedType(type);
		}
		private static bool IsAllowedType(Type t)
		{
			if (t != null && !t.IsAbstract && !t.IsInterface && !t.IsNotPublic)
				return true;

			if (typeof(IEnumerable<>).IsAssignableFrom(t))
				return true;

			return false;
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			var result = MessagePackSerializer.NonGeneric.Deserialize(type, readStream, _resolver);
			return Task.FromResult(result);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
			TransportContext transportContext)
		{
			MessagePackSerializer.NonGeneric.Serialize(type, writeStream, value, _resolver);
			return Task.CompletedTask;
		}
	}
}