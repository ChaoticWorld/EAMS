using EAMS.Application.Interfaces;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using System.Reflection;

namespace EAMS.Application.Services;

/// <summary>
/// 通用导入导出服务实现
/// </summary>
public class ImportExportService : IImportExportService
{
    /// <summary>
    /// 导出数据为Excel字节数组
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1")
    {
        using var stream = new MemoryStream();
        
        var config = new OpenXmlConfiguration
        {
            TableStyles = TableStyles.Default
        };
        
        await stream.SaveAsAsync(data, sheetName: sheetName, configuration: config);
        return stream.ToArray();
    }

    /// <summary>
    /// 从Excel流导入数据
    /// </summary>
    public async Task<IEnumerable<T>> ImportFromExcelAsync<T>(Stream stream, Action<T, int>? validateRow = null) where T : class, new()
    {
        var rows = (await stream.QueryAsync<T>(hasHeader: true)).ToList();
        
        // 行验证
        if (validateRow != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                validateRow(rows[i], i + 2); // Excel行号从2开始（第1行是表头）
            }
        }
        
        return rows;
    }
}
