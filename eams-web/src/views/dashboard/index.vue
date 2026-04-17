<template>
  <el-container class="layout-container">
    <!-- 侧边栏 -->
    <el-aside width="220px" class="sidebar">
      <div class="logo">
        <el-icon size="28"><Management /></el-icon>
        <span>EAMS</span>
      </div>

      <el-menu
        :default-active="$route.path"
        router
        background-color="#001529"
        text-color="#bfcbd9"
        active-text-color="#409EFF"
      >
        <el-menu-item index="/">
          <el-icon><HomeFilled /></el-icon>
          <span>首页</span>
        </el-menu-item>

        <el-sub-menu index="/system">
          <template #title>
            <el-icon><Setting /></el-icon>
            <span>系统管理</span>
          </template>
          <el-menu-item index="/users">
            <el-icon><User /></el-icon>
            <span>用户管理</span>
          </el-menu-item>
          <el-menu-item index="/roles">
            <el-icon><UserFilled /></el-icon>
            <span>角色管理</span>
          </el-menu-item>
          <el-menu-item index="/permissions">
            <el-icon><Key /></el-icon>
            <span>权限管理</span>
          </el-menu-item>
        </el-sub-menu>

        <el-sub-menu index="/org">
          <template #title>
            <el-icon><OfficeBuilding /></el-icon>
            <span>组织管理</span>
          </template>
          <el-menu-item index="/departments">
            <el-icon><School /></el-icon>
            <span>部门管理</span>
          </el-menu-item>
          <el-menu-item index="/employees">
            <el-icon><Avatar /></el-icon>
            <span>员工管理</span>
          </el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>

    <el-container>
      <!-- 顶部导航 -->
      <el-header class="header">
        <div class="header-left">
          <breadcrumb />
        </div>
        <div class="header-right">
          <!-- 消息通知 -->
          <el-popover
            placement="bottom-end"
            :width="360"
            trigger="click"
            v-model:visible="messagePopoverVisible"
          >
            <template #reference>
              <el-badge :value="unreadCount" :hidden="unreadCount === 0" :max="99">
                <el-button :icon="Bell" circle />
              </el-badge>
            </template>
            <div class="message-dropdown">
              <div class="message-header">
                <span class="message-title">消息通知</span>
                <el-button type="primary" link @click="handleMarkAllAsRead">全部已读</el-button>
              </div>
              <el-scrollbar max-height="300px">
                <div v-if="loadingMessages" class="message-loading">
                  <el-icon class="is-loading"><Loading /></el-icon>
                  加载中...
                </div>
                <template v-else-if="recentMessages.length > 0">
                  <div
                    v-for="msg in recentMessages"
                    :key="msg.id"
                    class="message-item"
                    :class="{ 'is-unread': !msg.isRead }"
                    @click="handleMarkAsRead(msg.id)"
                  >
                    <div class="message-content">
                      <div class="message-title-text">{{ msg.title }}</div>
                      <div class="message-desc">{{ msg.content }}</div>
                      <div class="message-time">{{ msg.createdAt.replace('T', ' ').slice(0, 16) }}</div>
                    </div>
                    <el-icon v-if="!msg.isRead" class="unread-dot"><StarFilled /></el-icon>
                  </div>
                </template>
                <div v-else class="message-empty">
                  <el-icon :size="32"><BellFilled /></el-icon>
                  <span>暂无消息</span>
                </div>
              </el-scrollbar>
              <div class="message-footer">
                <el-button type="primary" link @click="goToMessages">查看全部消息</el-button>
              </div>
            </div>
          </el-popover>

          <!-- 用户下拉 -->
          <el-dropdown @command="handleCommand">
            <span class="user-info">
              <el-avatar :size="32" :src="authStore.user?.avatar">
                {{ authStore.user?.realName?.charAt(0) }}
              </el-avatar>
              <span class="username">{{ authStore.user?.realName }}</span>
              <el-icon><ArrowDown /></el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="profile">个人中心</el-dropdown-item>
                <el-dropdown-item command="settings">系统设置</el-dropdown-item>
                <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 主内容区 -->
      <el-main class="main-content">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Bell, Loading, StarFilled, BellFilled, ArrowDown, HomeFilled, Setting, User, UserFilled, Key, Message as MessageIcon, Management, OfficeBuilding, School, Avatar } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { messageApi, type Message } from '@/api/message'

const router = useRouter()
const authStore = useAuthStore()

// 消息相关
const unreadCount = ref(0)
const messagePopoverVisible = ref(false)
const recentMessages = ref<Message[]>([])
const loadingMessages = ref(false)

// 获取未读消息数
const fetchUnreadCount = async () => {
  try {
    const res = await messageApi.getUnreadCount() as { count: number }
    unreadCount.value = res?.count ?? 0
  } catch (e) {
    console.error('获取未读消息数失败', e)
  }
}

// 获取最近消息
const fetchRecentMessages = async () => {
  loadingMessages.value = true
  try {
    const res = await messageApi.getMessages({ pageIndex: 1, pageSize: 5 }) as { items: Message[], total: number }
    recentMessages.value = res?.items ?? []
  } catch (e) {
    console.error('获取消息列表失败', e)
  } finally {
    loadingMessages.value = false
  }
}

// 标记单条已读
const handleMarkAsRead = async (id: number) => {
  try {
    await messageApi.markAsRead(id)
    const msg = recentMessages.value.find(m => m.id === id)
    if (msg) msg.isRead = true
    await fetchUnreadCount()
    ElMessage.success('已标记为已读')
  } catch (e) {
    ElMessage.error('操作失败')
  }
}

// 全部已读
const handleMarkAllAsRead = async () => {
  try {
    await messageApi.markAllAsRead()
    await fetchUnreadCount()
    await fetchRecentMessages()
    ElMessage.success('已全部标记为已读')
  } catch (e) {
    ElMessage.error('操作失败')
  }
}

// 跳转到消息管理页面
const goToMessages = () => {
  messagePopoverVisible.value = false
  router.push('/messages')
}

onMounted(() => {
  fetchUnreadCount()
  fetchRecentMessages()
})

const handleCommand = async (command: string) => {
  switch (command) {
    case 'profile':
      ElMessage.info('个人中心开发中...')
      break
    case 'settings':
      ElMessage.info('系统设置开发中...')
      break
    case 'logout':
      try {
        await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        await authStore.logout()
        ElMessage.success('已退出登录')
        router.push('/login')
      } catch {
        // 取消退出
      }
      break
  }
}
</script>

<style scoped>
.layout-container {
  height: 100vh;
}

.sidebar {
  background: #001529;
}

.logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  font-weight: bold;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.logo span {
  margin-left: 12px;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: white;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
}

.user-info {
  display: flex;
  align-items: center;
  cursor: pointer;
  padding: 8px;
  border-radius: 4px;
  transition: background 0.3s;
}

.user-info:hover {
  background: #f5f7fa;
}

.username {
  margin: 0 8px;
  color: #606266;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-right .el-button {
  margin: 0;
}

.message-dropdown {
  margin: -12px;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #ebeef5;
}

.message-title {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.message-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 20px;
  color: #909399;
}

.message-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 30px;
  color: #909399;
}

.message-item {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 12px 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.message-item:hover {
  background: #f5f7fa;
}

.message-item.is-unread {
  background: #f0f9ff;
}

.message-content {
  flex: 1;
  min-width: 0;
}

.message-title-text {
  font-size: 14px;
  color: #303133;
  font-weight: 500;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message-desc {
  font-size: 12px;
  color: #909399;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message-time {
  font-size: 12px;
  color: #c0c4cc;
}

.unread-dot {
  color: #f56c6c;
  margin-left: 8px;
  flex-shrink: 0;
}

.message-footer {
  padding: 8px 16px;
  border-top: 1px solid #ebeef5;
  text-align: center;
}

.main-content {
  background: #f0f2f5;
  padding: 20px;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
