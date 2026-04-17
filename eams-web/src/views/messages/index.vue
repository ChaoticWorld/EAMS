<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Delete, View, Check, Download, Upload } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { messageApi, type Message, type SendMessageRequest } from '@/api/message'
import { exportToExcel, importFromExcel, downloadTemplate } from '@/api/excel'
import { formatDateTime } from '@/utils/format'

// 搜索条件
const searchForm = ref({
  keyword: '',
  messageType: undefined as string | undefined,
  isRead: undefined as boolean | undefined
})

// 表格数据
const loading = ref(false)
const messageList = ref<Message[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const unreadCount = ref(0)

// 当前标签页
const activeTab = ref('all')

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('发送消息')
const formRef = ref<FormInstance>()
const form = ref<SendMessageRequest>({
  receiverId: 0,
  senderId: null,
  title: '',
  content: '',
  messageType: 'system'
})

const rules: FormRules = {
  title: [{ required: true, message: '请输入消息标题', trigger: 'blur' }],
  content: [{ required: true, message: '请输入消息内容', trigger: 'blur' }],
  messageType: [{ required: true, message: '请选择消息类型', trigger: 'change' }],
  receiverId: [{ 
    required: true, 
    validator: (rule, value, callback) => {
      if (form.value.messageType === 'personal' && !value) {
        callback(new Error('请选择接收人'))
      } else {
        callback()
      }
    }, 
    trigger: 'change' 
  }]
}

// 详情对话框
const detailVisible = ref(false)
const currentMessage = ref<Message | null>(null)

// 消息类型选项
const typeOptions = [
  { label: '系统通知', value: 'system' },
  { label: '个人消息', value: 'personal' },
  { label: '公告', value: 'announcement' }
]

// 用户选项（用于选择接收人）
const userOptions = ref([
  { id: 1, username: 'admin', realName: '管理员' }
])

// 获取当前用户ID
const currentUserId = computed(() => {
  const userStr = localStorage.getItem('user')
  if (userStr) {
    const user = JSON.parse(userStr)
    return user.id
  }
  return 1
})

// 加载数据
const loadData = async () => {
  loading.value = true
  try {
    // 根据标签页设置过滤条件
    let isRead: boolean | undefined = undefined
    if (activeTab.value === 'unread') {
      isRead = false
    } else if (activeTab.value === 'read') {
      isRead = true
    }
    
    const res = await messageApi.getMessages({
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      isRead,
      messageType: searchForm.value.messageType
    })
    
    // 关键词过滤（前端过滤，因为后端可能不支持）
    let items = res.items
    if (searchForm.value.keyword) {
      items = items.filter(m => m.title.includes(searchForm.value.keyword))
    }
    
    messageList.value = items
    total.value = res.total
    
    // 获取未读数
    unreadCount.value = await messageApi.getUnreadCount()
  } catch (error: any) {
    ElMessage.error(error || '加载失败')
  } finally {
    loading.value = false
  }
}

// 搜索
const handleSearch = () => {
  currentPage.value = 1
  loadData()
}

// 重置
const handleReset = () => {
  searchForm.value = { keyword: '', messageType: undefined, isRead: undefined }
  handleSearch()
}

// 标签切换
const handleTabChange = () => {
  currentPage.value = 1
  loadData()
}

// 新增
const handleAdd = () => {
  dialogTitle.value = '发送消息'
  form.value = {
    receiverId: 0,
    senderId: currentUserId.value,
    title: '',
    content: '',
    messageType: 'system'
  }
  dialogVisible.value = true
}

// 查看详情
const handleView = async (row: Message) => {
  currentMessage.value = row
  detailVisible.value = true
  // 标记为已读
  if (!row.isRead) {
    try {
      await messageApi.markAsRead(row.id)
      row.isRead = true
      row.readAt = new Date().toISOString()
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    } catch (error: any) {
      console.error('标记已读失败:', error)
    }
  }
}

// 标记已读
const handleMarkRead = async (row: Message) => {
  try {
    await messageApi.markAsRead(row.id)
    row.isRead = true
    row.readAt = new Date().toISOString()
    unreadCount.value = Math.max(0, unreadCount.value - 1)
    ElMessage.success('标记已读成功')
  } catch (error: any) {
    ElMessage.error(error || '操作失败')
  }
}

// 全部标记已读
const handleMarkAllRead = async () => {
  try {
    await messageApi.markAllAsRead()
    ElMessage.success('全部标记已读成功')
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '操作失败')
  }
}

// 删除
const handleDelete = async (row: Message) => {
  try {
    await ElMessageBox.confirm(`确定删除消息 "${row.title}" 吗？`, '提示', {
      type: 'warning'
    })
    await messageApi.deleteMessage(row.id)
    ElMessage.success('删除成功')
    loadData()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error || '删除失败')
    }
  }
}

// 保存
const handleSave = async () => {
  if (!formRef.value) return
  try {
    await formRef.value.validate()
    await messageApi.sendMessage(form.value)
    ElMessage.success('发送成功')
    dialogVisible.value = false
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '发送失败')
  }
}

// 获取类型标签
const getTypeLabel = (type: string) => {
  const map: Record<string, string> = { system: '系统', personal: '个人', announcement: '公告' }
  return map[type] || type
}

// 获取类型标签样式
const getTypeTagType = (type: string) => {
  const map: Record<string, any> = { system: 'primary', personal: 'success', announcement: 'warning' }
  return map[type] || 'info'
}

// 分页
const handleSizeChange = (val: number) => {
  pageSize.value = val
  loadData()
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
  loadData()
}

// 导出
const handleExport = () => {
  exportToExcel('/messages/export', '消息列表')
}

// 导入
const importLoading = ref(false)
const handleImport = async (options: { file: UploadFile }) => {
  importLoading.value = true
  try {
    const result = await importFromExcel('/messages/import', options.file)
    ElMessage.success(`导入完成，发送成功 ${result.successCount} 条`)
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '导入失败')
  } finally {
    importLoading.value = false
  }
}

// 下载模板
const handleDownloadTemplate = () => {
  downloadTemplate('/messages/template', '消息导入模板')
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="message-management">
    <!-- 搜索栏 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="searchForm" inline>
        <el-form-item label="消息标题">
          <el-input v-model="searchForm.keyword" placeholder="请输入消息标题" clearable />
        </el-form-item>
        <el-form-item label="消息类型">
          <el-select v-model="searchForm.messageType" placeholder="请选择类型" clearable style="width: 120px">
            <el-option label="系统通知" value="system" />
            <el-option label="个人消息" value="personal" />
            <el-option label="公告" value="announcement" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleSearch">查询</el-button>
          <el-button :icon="Refresh" @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 标签页 -->
    <el-card class="table-card" shadow="never">
      <template #header>
        <div class="card-header">
          <el-tabs v-model="activeTab" @tab-change="handleTabChange">
            <el-tab-pane label="全部消息" name="all" />
            <el-tab-pane name="unread">
              <template #label>
                <span>未读消息</span>
                <el-badge v-if="unreadCount > 0" :value="unreadCount" class="tab-badge" />
              </template>
            </el-tab-pane>
            <el-tab-pane label="已读消息" name="read" />
          </el-tabs>
          <div class="header-actions">
            <el-button v-if="unreadCount > 0" type="success" :icon="Check" @click="handleMarkAllRead">全部已读</el-button>
            <el-button type="primary" :icon="Plus" @click="handleAdd">发送消息</el-button>
            <el-button type="success" :icon="Download" @click="handleExport">导出</el-button>
            <el-upload
              :show-file-list="false"
              :before-upload="() => false"
              :on-change="handleImport"
              accept=".xlsx,.xls"
            >
              <el-button type="warning" :icon="Upload" :loading="importLoading">导入</el-button>
            </el-upload>
            <el-button :icon="Download" @click="handleDownloadTemplate">下载模板</el-button>
          </div>
        </div>
      </template>

      <!-- 表格 -->
      <el-table v-loading="loading" :data="messageList" border stripe>
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="title" label="消息标题" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            <el-badge is-dot :hidden="row.isRead" class="item">
              <span :class="{ 'unread': !row.isRead }">{{ row.title }}</span>
            </el-badge>
          </template>
        </el-table-column>
        <el-table-column prop="messageType" label="类型" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="getTypeTagType(row.messageType)" size="small">
              {{ getTypeLabel(row.messageType) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="senderName" label="发送人" width="120">
          <template #default="{ row }">
            {{ row.senderName || '系统' }}
          </template>
        </el-table-column>
        <el-table-column prop="receiverName" label="接收人" width="120">
          <template #default="{ row }">
            {{ row.receiverName || '全员' }}
          </template>
        </el-table-column>
        <el-table-column prop="isRead" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isRead ? 'info' : 'danger'" size="small">
              {{ row.isRead ? '已读' : '未读' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="发送时间" width="170">
          <template #default="{ row }">
            {{ formatDateTime(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link :icon="View" @click="handleView(row)">查看</el-button>
            <el-button v-if="!row.isRead" type="success" link :icon="Check" @click="handleMarkRead(row)">标为已读</el-button>
            <el-button type="danger" link :icon="Delete" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        class="pagination"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </el-card>

    <!-- 发送消息对话框 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="600px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="消息类型" prop="messageType">
          <el-radio-group v-model="form.messageType">
            <el-radio label="system">系统通知</el-radio>
            <el-radio label="personal">个人消息</el-radio>
            <el-radio label="announcement">公告</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="接收人" prop="receiverId" v-if="form.messageType === 'personal'">
          <el-select v-model="form.receiverId" placeholder="请选择接收人" clearable style="width: 100%">
            <el-option
              v-for="user in userOptions"
              :key="user.id"
              :label="`${user.realName} (${user.username})`"
              :value="user.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="消息标题" prop="title">
          <el-input v-model="form.title" placeholder="请输入消息标题" />
        </el-form-item>
        <el-form-item label="消息内容" prop="content">
          <el-input v-model="form.content" type="textarea" :rows="5" placeholder="请输入消息内容" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">发送</el-button>
      </template>
    </el-dialog>

    <!-- 消息详情对话框 -->
    <el-dialog v-model="detailVisible" title="消息详情" width="500px">
      <div v-if="currentMessage" class="message-detail">
        <div class="detail-item">
          <span class="label">消息标题：</span>
          <span class="value">{{ currentMessage.title }}</span>
        </div>
        <div class="detail-item">
          <span class="label">消息类型：</span>
          <el-tag :type="getTypeTagType(currentMessage.messageType)" size="small">
            {{ getTypeLabel(currentMessage.messageType) }}
          </el-tag>
        </div>
        <div class="detail-item">
          <span class="label">发送人：</span>
          <span class="value">{{ currentMessage.senderName || '系统' }}</span>
        </div>
        <div class="detail-item">
          <span class="label">接收人：</span>
          <span class="value">{{ currentMessage.receiverName || '全员' }}</span>
        </div>
        <div class="detail-item">
          <span class="label">发送时间：</span>
          <span class="value">{{ formatDateTime(currentMessage.createdAt) }}</span>
        </div>
        <div class="detail-item">
          <span class="label">阅读时间：</span>
          <span class="value">{{ currentMessage.readAt ? formatDateTime(currentMessage.readAt) : '-' }}</span>
        </div>
        <div class="detail-item">
          <span class="label">消息内容：</span>
        </div>
        <div class="content-box">
          {{ currentMessage.content }}
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped>
.message-management {
  padding: 20px;
}

.search-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}

.unread {
  font-weight: bold;
}

.tab-badge {
  margin-left: 5px;
}

.message-detail {
  padding: 10px;
}

.detail-item {
  margin-bottom: 15px;
  display: flex;
  align-items: center;
}

.detail-item .label {
  color: #606266;
  width: 80px;
  flex-shrink: 0;
}

.detail-item .value {
  color: #303133;
}

.content-box {
  background-color: #f5f7fa;
  padding: 15px;
  border-radius: 4px;
  line-height: 1.6;
  color: #303133;
}
</style>
