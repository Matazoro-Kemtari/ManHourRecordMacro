namespace Wada.Extensions;

public static class EnumExtension
{
    public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Exception exception)
        where T: Attribute
    {
        if (predicate(value)) throw exception;
        else return value;
    }

    public static string? GetEnumDisplayName<T>(this T enumValue)
        where T : Enum
    {
        return enumValue?.GetType()
            .GetField(enumValue.ToString()!)
            ?.GetCustomAttributes(typeof(EnumDisplayNameAttribute), false)
            .Cast<EnumDisplayNameAttribute>()
            .FirstOrDefault()
            ?.ThrowIf(a => a == null, new ArgumentException("属性が設定されていません"))
            .Name;
    }

    public static string? GetEnumDisplayShortName<T>(this T enumValue)
        where T : Enum
    {
        return enumValue?.GetType()
            .GetField(enumValue.ToString()!)
            ?.GetCustomAttributes(typeof(EnumDisplayShortNameAttribute), false)
            .Cast<EnumDisplayShortNameAttribute>()
            .FirstOrDefault()
            ?.ThrowIf(a => a == null, new ArgumentException("属性が設定されていません"))
            .Name;
    }
}
