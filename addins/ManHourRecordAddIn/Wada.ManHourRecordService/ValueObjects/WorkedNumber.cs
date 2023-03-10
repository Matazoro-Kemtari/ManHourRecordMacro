using System.Text.RegularExpressions;

namespace Wada.ManHourRecordService.ValueObjects;

public record class WorkingNumber(string Value)
{
    private static string Validate(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (!Regex.IsMatch(value, @"^X?\d{1,2}[A-Z]-\d{1,4}$"))
            throw new WorkingNumberException(
                $"正しい作業Noの形式を入力してください\n{value}");

        return value;
    }

    public override string ToString() => Value;

    private static string DivideHeader(string value)
    {
        var match = Regex.Match(value, @"\d{1,2}[A-Z]");
        return match.Success ? match.Value : string.Empty;
    }

    private static string DivideSymbol(string value)
    {
        var match = Regex.Match(value, @"(?<=\d{1,2})[A-Z]");
        return match.Success ? match.Value : string.Empty;
    }

    private static uint DivideNumber(string value)
    {
        var match = Regex.Match(value, @"(?<=-)\d{1,4}");
        return match.Success ? uint.Parse(match.Value) : default;
    }

    public string Value { get; } = Validate(Value);

    public string Header { get; } = DivideHeader(Value);

    public string Symbol { get; } = DivideSymbol(Value);

    public uint Number { get; } = DivideNumber(Value);
}

/// <summary>
/// テスト用インスタンス作成ファクトリ
/// </summary>
public class TestWorkingNumberFactory
{
    public static WorkingNumber Create(string value = "21G-300")
    {
        return new WorkingNumber(value);
    }
}