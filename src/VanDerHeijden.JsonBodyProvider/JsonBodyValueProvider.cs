using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;

namespace JsonBodyProvider;

internal class JsonBodyValueProvider(Dictionary<string, JsonNode?> values) : IValueProvider
{
	public bool ContainsPrefix(string prefix) => values.ContainsKey(prefix);

	public ValueProviderResult GetValue(string key)
	{
		// 1. Exact match for scalar or array
		if (values.TryGetValue(key, out JsonNode? node) && node is not null)
		{
			return ConvertToValueProviderResult(node);
		}

		// 2. Handle dictionary index-based binding (e.g. "Properties[0].Key", "Properties[0].Value", "Properties.index")
		if (key.Contains('[') && key.Contains(']'))
		{
			int openIndex = key.IndexOf('[');
			int closeIndex = key.IndexOf(']', openIndex);

			if (closeIndex > openIndex)
			{
				string propertyName = key[..openIndex];
				string indexStr = key[(openIndex + 1)..closeIndex];
				string subProperty = key[(closeIndex + 1)..];

				if (values.TryGetValue(propertyName, out JsonNode? dictNode) &&
					dictNode is JsonObject dictObj)
				{
					var kvList = dictObj.ToList(); // Preserve JSON order

					// Handle "prefix.index" → return indices as array ["0", "1", ...]
					if (subProperty == ".index")
					{
						if (kvList.Count == 0)
							return ValueProviderResult.None;

						string[] indices = Enumerable.Range(0, kvList.Count)
							.Select(i => i.ToString())
							.ToArray();

						return new ValueProviderResult(indices, CultureInfo.InvariantCulture);
					}

					// Handle "prefix[index].Key" or ".Value"
					if (subProperty.StartsWith(".") &&
						int.TryParse(indexStr, out int index) &&
						index >= 0 && index < kvList.Count)
					{
						var kv = kvList[index];

						if (subProperty == ".Key")
							return new ValueProviderResult(kv.Key, CultureInfo.InvariantCulture);

						if (subProperty == ".Value")
							return new ValueProviderResult(kv.Value?.ToString() ?? string.Empty, CultureInfo.InvariantCulture);
					}
				}
			}
		}
		return ValueProviderResult.None;
	}

	private static ValueProviderResult ConvertToValueProviderResult(JsonNode node)
	{
		if (node is JsonArray array)
		{
			string?[] valuesAsStrings = array.Select(item => item?.ToString()).ToArray()!;
			return new ValueProviderResult(valuesAsStrings, CultureInfo.InvariantCulture);
		}

		// Dictionaries: return None so binder uses index-based binding
		if (node is JsonObject)
		{
			return ValueProviderResult.None;
		}

		string? rawValue = node switch
		{
			JsonValue valueNode when valueNode.TryGetValue(out object? obj) => obj?.ToString(),
			_ => node.ToJsonString()
		};

		return new ValueProviderResult(rawValue ?? string.Empty, CultureInfo.InvariantCulture);
	}
}