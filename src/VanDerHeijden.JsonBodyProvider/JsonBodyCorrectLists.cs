using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Text.Json;

namespace VanDerHeijden.JsonBodyProvider;

public class EmptyStringPreservingBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context.Metadata.ModelType == typeof(string))
			return new EmptyStringPreservingStringBinder();

		return null;
	}
}

public class EmptyStringPreservingStringBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext context)
	{
		var value = context.ValueProvider.GetValue(context.ModelName);

		if (value == ValueProviderResult.None)
			return Task.CompletedTask;

		context.Result = ModelBindingResult.Success(value.FirstValue);
		return Task.CompletedTask;
	}
}

public class NoFallbackListBinder<T> : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext context)
	{
		var result = new List<T?>();
		var value = context.ValueProvider.GetValue(context.ModelName);

		if (value != ValueProviderResult.None)
		{
			foreach (var v in value)
			{
				// null → null
				if (v == null)
				{
					result.Add(default);
					continue;
				}

				var underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

				// string
				if (underlying == typeof(string))
				{
					result.Add((T?)(object)v);
					continue;
				}

				// empty → default (line a), except nullable → default = null? no = 0/false (a/b)
				if (v == "")
				{
					result.Add((T?)DefaultValueFor(underlying));
					continue;
				}

				// parse to type
				result.Add((T?)ParseValue(v, underlying));
			}

			context.Result = ModelBindingResult.Success(result);
			return Task.CompletedTask;
		}

		context.Result = ModelBindingResult.Success(result);
		return Task.CompletedTask;
	}

	private static object? ParseValue(string v, Type type) => type switch
	{
		// === STRING ===
		_ when type == typeof(string) => v,

		// === GUID ===
		_ when type == typeof(Guid) => v == "" || !Guid.TryParse(v, out var g) ? Guid.Empty : g,

		// === DATETIME ===
		_ when type == typeof(DateTime) => v == "" || !DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? default(DateTime) : dt,

		// === DATEONLY ===
		_ when type == typeof(DateOnly) => v == "" || !DateOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? default(DateOnly) : d,

		// === TIMEONLY ===
		_ when type == typeof(TimeOnly) => v == "" || !TimeOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? default(TimeOnly) : t,

		// === BOOL ===
		_ when type == typeof(bool) => v.Equals("true", StringComparison.OrdinalIgnoreCase),

		// === ENUM ===
		_ when type.IsEnum => Enum.TryParse(type, v, true, out var parsed) ? parsed : Activator.CreateInstance(type)!,

		// === NUMERIC + COMPLEX TYPES ===
		_ => JsonSerializer.Deserialize(v, type, options)
	};

	private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

	private static object? DefaultValueFor(Type type)
	{
		if (type == typeof(string)) return "";
		if (type == typeof(bool)) return false;
		if (type.IsEnum) return Activator.CreateInstance(type);
		return Activator.CreateInstance(type); // numerics → 0
	}
}

public class NoFallbackListBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context.Metadata.BindingSource == BindingSource.Body)
		{
			return null;
		}

		if (context.Metadata.ModelType.IsGenericType &&
			context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(List<>))
		{
			var elementType = context.Metadata.ModelType.GetGenericArguments()[0];
			var binderType = typeof(NoFallbackListBinder<>).MakeGenericType(elementType);
			return (IModelBinder)Activator.CreateInstance(binderType)!;
		}

		return null;
	}
}
