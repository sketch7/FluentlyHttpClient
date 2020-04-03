using HeyRed.Mime;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	public static class MultipartFormDataContentExtensions
	{
		/// <summary>
		/// Add file async from path.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="filePath">Filepath to read from.</param>
		public static async Task AddFileAsync(this MultipartFormDataContent multipartForm, string name, string filePath)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));

			var fileName = Path.GetFileName(filePath);
			byte[] fileBytes;
			using (var fileStream = File.Open(filePath, FileMode.Open))
			{
				fileBytes = new byte[fileStream.Length];
				await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
			}
			//var fileBytes = await File.ReadAllBytesAsync(filePath); // todo: netstandard2.1
			var fileMimeType = MimeTypesMap.GetMimeType(fileName);
			var fileArrayContent = new ByteArrayContent(fileBytes);
			fileArrayContent.Headers.ContentType = new MediaTypeHeaderValue(fileMimeType);

			multipartForm.Add(fileArrayContent, name, fileName);
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
		/// Add file from stream.
		/// </summary>
		/// <param name="multipartForm"></param>
		/// <param name="name">Name to use for the http form parameter.</param>
		/// <param name="stream"></param>
		/// <param name="fileName"></param>
		/// <param name="mimeType"></param>
		public static void AddFile(this MultipartFormDataContent multipartForm, string name, Stream stream, string fileName, string mimeType)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			//var fileMimeType = MimeTypesMap.GetMimeType(mimeType);
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