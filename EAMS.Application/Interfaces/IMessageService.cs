using EAMS.Application.DTOs;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 消息服务接口
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// 发送消息
    /// </summary>
    Task<MessageDto> SendMessageAsync(SendMessageRequestDto request);
    
    /// <summary>
    /// 批量发送消息
    /// </summary>
    Task SendBatchMessageAsync(SendBatchMessageRequestDto request);
    
    /// <summary>
    /// 获取用户消息列表
    /// </summary>
    Task<PagedResult<MessageDto>> GetUserMessagesAsync(long userId, MessageQueryRequestDto query);
    
    /// <summary>
    /// 获取未读消息数
    /// </summary>
    Task<int> GetUnreadCountAsync(long userId);
    
    /// <summary>
    /// 标记消息已读
    /// </summary>
    Task MarkAsReadAsync(long messageId);
    
    /// <summary>
    /// 标记所有消息已读
    /// </summary>
    Task MarkAllAsReadAsync(long userId);
    
    /// <summary>
    /// 删除消息
    /// </summary>
    Task DeleteMessageAsync(long messageId, long userId);
    
    /// <summary>
    /// 发送系统通知
    /// </summary>
    Task SendSystemNotificationAsync(string title, string content, IEnumerable<long>? targetUserIds = null);
}
