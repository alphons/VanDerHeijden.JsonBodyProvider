using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Nodes;

namespace JsonBodyProvider;

internal class JsonBodyValueProviderFactory : IValueProviderFactory
{
	public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
	{
		HttpRequest request = context.ActionContext.HttpContext.Request;

		Dictionary<string, JsonNode?> values = [];

		if (IsJsonContentType(request) && request.ContentLength.GetValueOrDefault() >= 2)
		{
			request.EnableBuffering();

			using var reader = new StreamReader(request.Body, leaveOpen: true);

			var json = await reader.ReadToEndAsync(context.ActionContext.HttpContext.RequestAborted);

			if (JsonNode.Parse(json) is JsonObject obj)
			{
				foreach (var kv in obj)
					values[kv.Key] = kv.Value;
			}

			request.Body.Position = 0;
		}

		foreach (var header in request.Headers)
		{
			if (header.Value.FirstOrDefault() is { } value)
				values.TryAdd(header.Key, JsonValue.Create(value));
		}

		foreach (var cookie in request.Cookies)
			values.TryAdd(cookie.Key, JsonValue.Create(cookie.Value));

		context.ValueProviders.Add(new JsonBodyValueProvider(values));
	}

	private static bool IsJsonContentType(HttpRequest request) =>
		request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
}
