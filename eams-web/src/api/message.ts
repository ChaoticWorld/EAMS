import api from './request'

export interface Message {
  id: number
  receiverId: number
  receiverName: string
  senderId: number | null
  senderName: string | null
  title: string
  content: string
  messageType: string  // 'system' | 'personal' | 'announcement'
  isRead: boolean
  readAt: string | null
  businessId: string | null
  businessType: string | null
  createdAt: string
}

export interface MessageQuery {
  pageIndex?: number
  pageSize?: number
  isRead?: boolean
  messageType?: string
}

export interface SendMessageRequest {
  receiverId: number
  senderId?: number | null
  title: string
  content: string
  messageType: string
  businessId?: string
  businessType?: string
}

export interface SendBatchMessageRequest {
  receiverIds: number[]
  senderId?: number | null
  title: string
  content: string
  messageType: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

export const messageApi = {
  // 获取消息列表
  getMessages: (params?: MessageQuery) => 
    api.get<PagedResult<Message>>('/messages', { params }),
  
  // 获取未读消息数
  getUnreadCount: () => 
    api.get<number>('/messages/unread-count'),
  
  // 标记单条消息已读
  markAsRead: (id: number) => 
    api.put(`/messages/${id}/read`),
  
  // 标记所有消息已读
  markAllAsRead: () => 
    api.put('/messages/read-all'),
  
  // 删除消息
  deleteMessage: (id: number) => 
    api.delete(`/messages/${id}`),
  
  // 发送消息
  sendMessage: (data: SendMessageRequest) => 
    api.post<Message>('/messages', data),
  
  // 批量发送消息
  sendBatchMessage: (data: SendBatchMessageRequest) => 
    api.post('/messages/batch', data)
}
