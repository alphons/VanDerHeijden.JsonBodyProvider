using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace JsonBodyProvider;

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

				// empty → default (regel a), behalve nullable → default = null? nee = 0/false (a/b)
				if (v == "")
				{
					result.Add((T?)DefaultValueFor(underlying));
					continue;
				}

				// parse naar type
				result.Add((T?)ParseValue(v, underlying));
			}

			context.Result = ModelBindingResult.Success(result);
			return Task.CompletedTask;
		}

		context.Result = ModelBindingResult.Success(result);
		return Task.CompletedTask;
	}

	private static object? ParseValue(string v, Type type)
	{
		// === GUID ===
		if (type == typeof(Guid))
		{
			if (v == "") return Guid.Empty;
			if (Guid.TryParse(v, out var g)) return g;
			return Guid.Empty; // unknown → default
		}

		// === DATETIME === (ISO / invariant)
		if (type == typeof(DateTime))
		{
			if (v == "") return default(DateTime);
			if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
				return dt;
			return default(DateTime); // unknown
		}

		// === DATEONLY ===
		if (type == typeof(DateOnly))
		{
			if (v == "") return default(DateOnly);
			if (DateOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
				return d;
			return default(DateOnly);
		}

		// === TIMEONLY ===
		if (type == typeof(TimeOnly))
		{
			if (v == "") return default(TimeOnly);
			if (TimeOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
				return t;
			return default(TimeOnly);
		}

		// verder zoals je had ↓↓↓

		if (type == typeof(double)) return double.Parse(v, CultureInfo.InvariantCulture);
		if (type == typeof(float)) return float.Parse(v, CultureInfo.InvariantCulture);
		if (type == typeof(decimal)) return decimal.Parse(v, CultureInfo.InvariantCulture);
		if (type == typeof(int)) return int.Parse(v, CultureInfo.InvariantCulture);
		if (type == typeof(long)) return long.Parse(v, CultureInfo.InvariantCulture);
		if (type == typeof(short)) return short.Parse(v, CultureInfo.InvariantCulture);

		if (type == typeof(bool))
		{
			if (v.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
			if (v.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
			return false;
		}

		if (type.IsEnum)
		{
			if (Enum.TryParse(type, v, true, out var parsed))
				return parsed;
			return Activator.CreateInstance(type)!;
		}

		// fallback
		return Convert.ChangeType(v, type, CultureInfo.InvariantCulture);
	}


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
