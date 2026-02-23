using Microsoft.Extensions.DependencyInjection;

namespace JsonBodyProvider;

public static class JsonBodyExtensions
{
	public static IServiceCollection AddJsonProviders(this IServiceCollection services, bool CorrectLists = false)
	{
		return services
			.AddControllers(options =>
			{
				options.ValueProviderFactories.Add(new JsonBodyValueProviderFactory());
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
