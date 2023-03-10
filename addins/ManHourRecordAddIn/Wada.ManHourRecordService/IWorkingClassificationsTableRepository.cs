namespace Wada.ManHourRecordService
{
    public interface IWorkingClassificationsTableRepository
    {
        IEnumerable<IEnumerable<object?>> FetchAll(Stream stream);
    }
}
