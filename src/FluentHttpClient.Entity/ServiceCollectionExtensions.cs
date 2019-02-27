using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHttpClient.Entity
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFluentHttpClientEntity(this IServiceCollection services, string connectionString = "")
		{
			//var connection = @"Server=(LocalDb)\MSSQLLocalDB;Database=FluentHttpClient;Integrated Security=True;";
			//var connection = @"Server=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\FluentHttpClient.mdf;Initial Catalog=FluentHttpClient;Integrated Security=SSPI;";
			//var connection = @"Server=(LocalDb)\MSSQLLocalDB;Initial Catalog=FluentHttpClient;AttachDbFileName=c:\FluentHttpClient.mdf;Integrated Security=True;MultipleActiveResultSets=True";
			var connString = @"Data Source=.\SQLEXPRESS;Database=FluentHttpClient;Integrated Security=True";

			if (connectionString != string.Empty)
				connString = connectionString;

			return services.AddDbContext<FluentHttpClientContext>(options => options.UseSqlServer(connString));
		}
	}
}