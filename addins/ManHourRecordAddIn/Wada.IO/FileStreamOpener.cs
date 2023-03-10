using Wada.AOP.Logging;
using Wada.ManHourRecordService;

namespace Wada.IO;

public class FileStreamOpener : IFileStreamOpener
{
    [Logging]
    public Stream OpenOrCreate(string path)
    {
        try
        {
            return OpenFileStream(path);
        }
        catch (IOException ex)
        {
            string msg = "ファイルが使用中です";
            throw new FileStreamOpenerException(msg, ex);
        }
    }

    [Logging]
    private static FileStream OpenFileStream(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        // ファイルの存在を確認
        if (!fileInfo.Exists)
        {
            if (fileInfo.Directory != null
                && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
        }

        return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
    }
}