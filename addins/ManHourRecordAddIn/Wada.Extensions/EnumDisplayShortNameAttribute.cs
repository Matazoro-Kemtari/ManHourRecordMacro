namespace Wada.Extensions;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumDisplayShortNameAttribute : Attribute
{
    public string? Name { get; set; }

    public EnumDisplayShortNameAttribute(string? name = default)
    {
        Name = name;
    }
}
