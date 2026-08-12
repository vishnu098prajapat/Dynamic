using ReportModel;
using System.Threading.Tasks;
/// <summary>
/// Summary description for IReportService
/// </summary>
public interface IReportService<T>
{
    Task<T> GetReport(HistorySearchModel historySearchModel,string MemberID);
}