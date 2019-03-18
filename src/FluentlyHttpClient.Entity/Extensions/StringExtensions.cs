using System.Data.HashFunction.xxHash;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient.Entity
{
	// todo: extract to core library
	public static class StringExtensions
	{
		private static readonly IxxHash HashFunction = xxHashFactory.Instance.Create();

		public static async Task<string> ComputeHash(this string text)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				var value = await HashFunction.ComputeHashAsync(stream);
				return value.AsBase64String();
			}
		}

		public static string ComputeHashSync(this string text)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				var value = HashFunction.ComputeHash(stream);
				return value.AsBase64String();
			}
		}
	}
}
