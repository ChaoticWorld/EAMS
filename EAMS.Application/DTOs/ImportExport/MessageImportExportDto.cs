using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 消息导出DTO
/// </summary>
public class MessageExportDto
{
    [ExcelColumnName("消息ID")]
    public long Id { get; set; }

    [ExcelColumnName("标题")]
    public string Title { get; set; } = string.Empty;

    [ExcelColumnName("内容")]
    public string? Content { get; set; }

    [ExcelColumnName("类型")]
    public string TypeText => Type switch
    {
        1 => "系统通知",
        2 => "待办提醒",
        3 => "审批通知",
        _ => "其他"
    };

    [ExcelIgnore]
    public int Type { get; set; }

    [ExcelColumnName("接收人ID")]
    public long ReceiverId { get; set; }

    [ExcelColumnName("是否已读")]
    public string IsReadText => IsRead ? "是" : "否";

    [ExcelIgnore]
    public bool IsRead { get; set; }

    [ExcelColumnName("发送时间")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 消息导入DTO
/// </summary>
public class MessageImportDto
{
    [ExcelColumnName("标题")]
    public string Title { get; set; } = string.Empty;

    [ExcelColumnName("内容")]
    public string? Content { get; set; }

    [ExcelColumnName("类型")]
    public string? TypeText { get; set; }

    [ExcelColumnName("接收人ID")]
    public long ReceiverId { get; set; }
}
