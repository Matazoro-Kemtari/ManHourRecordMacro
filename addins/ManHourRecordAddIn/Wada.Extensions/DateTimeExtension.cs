namespace Wada.Extensions;

public static class DateTimeExtension
{
    /// <summary>
    /// 4月始まりの年度を取得する
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int FiscalYear(this DateTime date)
        => date.Month <= 3 ? date.Year - 1 : date.Year;
}
