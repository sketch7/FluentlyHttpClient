using Newtonsoft.Json;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluentHttpClient.Entity.Extensions
{
	// todo: extract to core library
	public static class ObjectExtensions
	{
		private static readonly IxxHash HashFunction = xxHashFactory.Instance.Create();

		public static async Task<string> ComputeHash(this object obj)
		{
			var serialized = JsonConvert.SerializeObject(obj);
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
			{
				var value = await HashFunction.ComputeHashAsync(stream);
				return value.AsBase64String();
			}
		}

		public static string ComputeHashSync(this object obj)
		{
			var serialized = JsonConvert.SerializeObject(obj); // todo: investigate more performant serialization. If it is really more performant
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
			{
				var value = HashFunction.ComputeHash(stream);
				return value.AsBase64String();
			}
		}
	}
}
