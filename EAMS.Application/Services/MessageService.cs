using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EAMS.Application.Services;

/// <summary>
/// 消息服务实现
/// </summary>
public class MessageService : IMessageService
{
    private readonly IRepository<SysMessage, long> _messageRepository;
    private readonly IRepository<SysUser, long> _userRepository;

    public MessageService(
        IRepository<SysMessage, long> messageRepository,
        IRepository<SysUser, long> userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageRequestDto request)
    {
        var message = new SysMessage
        {
            ReceiverId = request.ReceiverId,
            SenderId = request.SenderId,
            Title = request.Title,
            Content = request.Content,
            MessageType = request.MessageType,
            BusinessId = request.BusinessId,
            BusinessType = request.BusinessType,
            IsRead = false
        };

        await _messageRepository.AddAsync(message);
        return await MapToDtoAsync(message);
    }

    public async Task SendBatchMessageAsync(SendBatchMessageRequestDto request)
    {
        foreach (var receiverId in request.ReceiverIds)
        {
            var message = new SysMessage
            {
                ReceiverId = receiverId,
                SenderId = request.SenderId,
                Title = request.Title,
                Content = request.Content,
                MessageType = request.MessageType,
                IsRead = false
            };
            await _messageRepository.AddAsync(message);
        }
    }

    public async Task<PagedResult<MessageDto>> GetUserMessagesAsync(long userId, MessageQueryRequestDto query)
    {
        var q = _messageRepository.GetQueryable()
            .Where(m => m.ReceiverId == userId);

        // 已读状态筛选
        if (query.IsRead.HasValue)
        {
            q = q.Where(m => m.IsRead == query.IsRead.Value);
        }

        // 消息类型筛选
        if (!string.IsNullOrWhiteSpace(query.MessageType))
        {
            q = q.Where(m => m.MessageType == query.MessageType);
        }

        // 排序：未读在前，然后按时间倒序
        q = q.OrderBy(m => m.IsRead).ThenByDescending(m => m.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = new List<MessageDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToDtoAsync(item));
        }

        return new PagedResult<MessageDto>
        {
            Items = dtos,
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _messageRepository.GetQueryable()
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .CountAsync();
    }

    public async Task MarkAsReadAsync(long messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new InvalidOperationException("消息不存在");

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _messageRepository.UpdateAsync(message);
        }
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        var unreadMessages = await _messageRepository.FindListAsync(
            m => m.ReceiverId == userId && !m.IsRead);

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _messageRepository.UpdateAsync(message);
        }
    }

    public async Task DeleteMessageAsync(long messageId, long userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new InvalidOperationException("消息不存在");

        if (message.ReceiverId != userId)
            throw new InvalidOperationException("无权删除此消息");

        await _messageRepository.DeleteAsync(message);
    }

    public async Task SendSystemNotificationAsync(string title, string content, IEnumerable<long>? targetUserIds = null)
    {
        if (targetUserIds == null || !targetUserIds.Any())
        {
            // 发送给所有用户
            var allUserIds = await _userRepository.GetQueryable()
                .Where(u => u.IsEnabled && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in allUserIds)
            {
                await _messageRepository.AddAsync(new SysMessage
                {
                    ReceiverId = userId,
                    Title = title,
                    Content = content,
                    MessageType = "system",
                    IsRead = false
                });
            }
        }
        else
        {
            foreach (var userId in targetUserIds)
            {
                await _messageRepository.AddAsync(new SysMessage
                {
                    ReceiverId = userId,
                    Title = title,
                    Content = content,
                    MessageType = "system",
                    IsRead = false
                });
            }
        }
    }

    private async Task<MessageDto> MapToDtoAsync(SysMessage message)
    {
        string receiverName = string.Empty;
        string? senderName = null;

        var receiver = await _userRepository.GetByIdAsync(message.ReceiverId);
        if (receiver != null)
            receiverName = receiver.RealName ?? receiver.Username;

        if (message.SenderId.HasValue)
        {
            var sender = await _userRepository.GetByIdAsync(message.SenderId.Value);
            if (sender != null)
                senderName = sender.RealName ?? sender.Username;
        }

        return new MessageDto
        {
            Id = message.Id,
            ReceiverId = message.ReceiverId,
            ReceiverName = receiverName,
            SenderId = message.SenderId,
            SenderName = senderName,
            Title = message.Title,
            Content = message.Content,
            MessageType = message.MessageType,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            BusinessId = message.BusinessId,
            BusinessType = message.BusinessType,
            CreatedAt = message.CreatedAt
        };
    }
}
