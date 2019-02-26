namespace FluentHttpClient.Entity.Models
{
	public class HttpRequest
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public string Url { get; set; }
		public string Method { get; set; }
		public string Headers { get; set; }
		public int ResponseCode { get; set; }
		public string Content { get; set; }
	}
}