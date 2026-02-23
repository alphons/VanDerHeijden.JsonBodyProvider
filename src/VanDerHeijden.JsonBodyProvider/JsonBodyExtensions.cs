using Microsoft.Extensions.DependencyInjection;

namespace VanDerHeijden.JsonBodyProvider;

public static class JsonBodyExtensions
{
	public static IServiceCollection AddJsonBodyProvider(this IServiceCollection services, 
		bool CorrectLists = true, 
		bool ParseHeaders = false, 
		bool ParseCookies = false)
	{
		return services
			.AddControllers(options =>
			{
				options.ValueProviderFactories.Add(new JsonBodyValueProviderFactory(ParseHeaders, ParseCookies));
				if (CorrectLists)
				{
					options.ModelBinderProviders.Insert(0, new NoFallbackListBinderProvider());
					options.ModelBinderProviders.Insert(0, new EmptyStringPreservingBinderProvider());
				}
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
				options.JsonSerializerOptions.PropertyNamingPolicy = null;
				options.JsonSerializerOptions.DictionaryKeyPolicy = null;
			})
			.Services;
	}
}
