namespace Wada.ManHourRecordService;

public interface IFileStreamOpener
{
    /// <summary>
    /// ファイルストリームを開く
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Stream OpenOrCreate(string path);
}
