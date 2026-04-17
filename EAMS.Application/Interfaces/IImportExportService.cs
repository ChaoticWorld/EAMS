namespace EAMS.Application.Interfaces;

/// <summary>
/// 通用导入导出服务接口
/// </summary>
public interface IImportExportService
{
    /// <summary>
    /// 导出数据为Excel字节数组
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1");

    /// <summary>
    /// 从Excel流导入数据
    /// </summary>
    Task<IEnumerable<T>> ImportFromExcelAsync<T>(Stream stream, Action<T, int>? validateRow = null) where T : class, new();
}
