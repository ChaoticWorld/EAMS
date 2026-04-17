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

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
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
}
