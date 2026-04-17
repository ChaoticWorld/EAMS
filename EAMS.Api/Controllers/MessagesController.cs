using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 消息管理控制器
/// </summary>
[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : BaseController
{
    private readonly IMessageService _messageService;
    private readonly IImportExportService _importExportService;

    public MessagesController(IMessageService messageService, IImportExportService importExportService)
    {
        _messageService = messageService;
        _importExportService = importExportService;
    }

    /// <summary>
    /// 获取用户消息列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] MessageQueryRequestDto query)
    {
        var result = await _messageService.GetUserMessagesAsync(CurrentUserId, query);
        return Success(result);
    }

    /// <summary>
    /// 获取未读消息数
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _messageService.GetUnreadCountAsync(CurrentUserId);
        return Success(new { count });
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
    {
        var message = await _messageService.SendMessageAsync(request);
        return Success(message, "发送成功");
    }

    /// <summary>
    /// 批量发送消息
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> SendBatchMessage([FromBody] SendBatchMessageRequestDto request)
    {
        await _messageService.SendBatchMessageAsync(request);
        return Success<object>(null, "批量发送成功");
    }

    /// <summary>
    /// 标记消息已读
    /// </summary>
    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        await _messageService.MarkAsReadAsync(id);
        return Success<object>(null, "已标记为已读");
    }

    /// <summary>
    /// 标记所有消息已读
    /// </summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _messageService.MarkAllAsReadAsync(CurrentUserId);
        return Success<object>(null, "全部已读");
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteMessage(long id)
    {
        await _messageService.DeleteMessageAsync(id, CurrentUserId);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 导出消息（管理员）
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportMessages([FromQuery] MessageQueryRequestDto query)
    {
        var result = await _messageService.GetUserMessagesAsync(CurrentUserId, query);
        var exportData = result.Items.Select(m => new MessageExportDto
        {
            Id = m.Id,
            Title = m.Title,
            Content = m.Content,
            Type = m.MessageType?.Trim() switch
            {
                "system" => 1,
                "todo" => 2,
                "approval" => 3,
                _ => 0
            },
            ReceiverId = m.ReceiverId,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "消息列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"消息列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 批量导入发送消息
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportMessages(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<MessageImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var type = row.TypeText?.Trim() switch
                {
                    "系统通知" => "system",
                    "待办提醒" => "todo",
                    "审批通知" => "approval",
                    _ => "user"
                };
                await _messageService.SendMessageAsync(new SendMessageRequestDto
                {
                    Title = row.Title,
                    Content = row.Content,
                    MessageType = type,
                    ReceiverId = row.ReceiverId
                });
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"第{index}行: {ex.Message}");
            }
        }

        return Success(new { successCount, errorCount = errors.Count, errors }, $"导入完成，发送成功{successCount}条");
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var template = new List<MessageImportDto>
        {
            new MessageImportDto { Title = "示例消息标题", Content = "消息内容", TypeText = "系统通知", ReceiverId = 1 }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "消息导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "消息导入模板.xlsx");
    }
}
