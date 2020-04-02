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
			var fileBytes = await File.ReadAllBytesAsync(filePath);
			var fileMimeType = MimeTypesMap.GetMimeType(fileName);

			var fileArrayContent = new ByteArrayContent(fileBytes);
			fileArrayContent.Headers.ContentType = new MediaTypeHeaderValue(fileMimeType);

			multipartForm.Add(fileArrayContent, name, fileName);
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