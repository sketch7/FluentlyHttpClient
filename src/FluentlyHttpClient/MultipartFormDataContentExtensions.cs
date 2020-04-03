using HeyRed.Mime;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FluentlyHttpClient
{
	public static class MultipartFormDataContentExtensions
	{
		/// <summary>
		/// Add file as from path as StreamContent.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="filePath">Filepath to read from.</param>
		public static void AddFile(this MultipartFormDataContent multipartForm, string name, string filePath)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));

			var fileStream = File.OpenRead(filePath);
			multipartForm.AddFile(name, fileStream);
		}

		/// <summary>
		/// Add file from file stream.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="stream">File stream to send.</param>
		public static void AddFile(this MultipartFormDataContent multipartForm, string name, FileStream stream)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			//stream.Position = 0;
			var fileName = Path.GetFileName(stream.Name);
			var fileMimeType = MimeTypesMap.GetMimeType(fileName);
			multipartForm.AddFile(name, stream, fileName, fileMimeType);
		}

		/// <summary>
		/// Add file from bytes.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="fileBytes"></param>
		/// <param name="fileName"></param>
		/// <param name="mimeType"></param>
		public static void AddFile(this MultipartFormDataContent multipartForm, string name, byte[] fileBytes, string fileName, string? mimeType = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var memory = new MemoryStream(fileBytes);
			multipartForm.AddFile(name, memory, fileName, mimeType);
		}

		/// <summary>
		/// Add file from stream.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="stream"></param>
		/// <param name="fileName"></param>
		/// <param name="mimeType"></param>
		public static void AddFile(this MultipartFormDataContent multipartForm, string name, Stream stream, string fileName, string? mimeType = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (mimeType == null)
			{
				var fileExt = Path.GetExtension(fileName);
				if (string.IsNullOrEmpty(fileExt))
					throw new ArgumentException($"'{nameof(mimeType)}' needs to be specified or '{nameof(fileName)}' must contains extension.");
				mimeType = MimeTypesMap.GetMimeType(fileName);
			}

			var fileStreamContent = new StreamContent(stream);
			fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

			multipartForm.Add(fileStreamContent, name, fileName);
		}


		/// <summary>
		/// Add string content.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="value">Value to use for string content.</param>
		public static void Add(this MultipartFormDataContent multipartForm, string name, string value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			multipartForm.Add(new StringContent(value), name);
		}

	}
}