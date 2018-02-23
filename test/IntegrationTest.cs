using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test
{


	public class IntegrationTest
	{
		private readonly MessagePackMediaTypeFormatter _messagePackMediaTypeFormatter = new MessagePackMediaTypeFormatter(ContractlessStandardResolver.Instance);

		// todo: disable test
		[Fact]
		public async void ShouldMakeRequest_Get()
		{

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
				.WithFormatters(x => x.Add(_messagePackMediaTypeFormatter))
				;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async void ShouldMakeRequest_Post()
		{

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
				.WithFormatters(x => x.Add(_messagePackMediaTypeFormatter))
				;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Ucrowned"
				}, _messagePackMediaTypeFormatter)
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}
	}

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
			var r = MessagePackSerializer.NonGeneric.Deserialize(type, readStream, _resolver);
			return Task.FromResult(r);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
			TransportContext transportContext)
		{
			MessagePackSerializer.NonGeneric.Serialize(type, writeStream, value, _resolver);
			return Task.CompletedTask;
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
			TransportContext transportContext, CancellationToken cancellationToken)
		{
			return base.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
		}
	}
}
