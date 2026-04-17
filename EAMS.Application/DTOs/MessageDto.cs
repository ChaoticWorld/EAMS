namespace EAMS.Application.DTOs;

// ========== 消息发送 ==========
public class SendMessageRequestDto
{
    public long ReceiverId { get; set; }
    public long? SenderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "user";
    public long? BusinessId { get; set; }
    public string? BusinessType { get; set; }
}

public class SendBatchMessageRequestDto
{
    public IEnumerable<long> ReceiverIds { get; set; } = new List<long>();
    public long? SenderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "system";
}

public class MessageQueryRequestDto : PagedRequestDto
{
    public bool? IsRead { get; set; }
    public string? MessageType { get; set; }
}

// ========== 消息响应 ==========
public class MessageDto
{
    public long Id { get; set; }
    public long ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public long? SenderId { get; set; }
    public string? SenderName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public long? BusinessId { get; set; }
    public string? BusinessType { get; set; }
    public DateTime CreatedAt { get; set; }
}
