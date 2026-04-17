<template>
  <div class="home">
    <!-- 统计卡片 -->
    <el-row :gutter="20">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon size="48" color="#409EFF"><User /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ stats.users }}</div>
              <div class="stat-label">用户总数</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon size="48" color="#67C23A"><UserFilled /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ stats.roles }}</div>
              <div class="stat-label">角色数量</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon size="48" color="#E6A23C"><Key /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ stats.permissions }}</div>
              <div class="stat-label">权限数量</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon size="48" color="#F56C6C"><Message /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ stats.messages }}</div>
              <div class="stat-label">未读消息</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 欢迎信息 -->
    <el-card class="welcome-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>欢迎使用 EAMS</span>
        </div>
      </template>
      <div class="welcome-content">
        <h3>👋 你好，{{ authStore.user?.realName }}</h3>
        <p>今天是 {{ today }}</p>
        <p>当前时间：{{ currentTime }}</p>
        <el-divider />
        <p class="tips">💡 提示：您可以通过左侧菜单访问各个功能模块</p>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const stats = ref({
  users: 0,
  roles: 0,
  permissions: 0,
  messages: 0
})

const today = ref('')
const currentTime = ref('')
let timer: number

const updateTime = () => {
  const now = new Date()
  today.value = now.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  })
  currentTime.value = now.toLocaleTimeString('zh-CN')
}

onMounted(() => {
  updateTime()
  timer = window.setInterval(updateTime, 1000)

  // TODO: 从 API 获取统计数据
  stats.value = {
    users: 12,
    roles: 5,
    permissions: 28,
    messages: 3
  }
})

onUnmounted(() => {
  clearInterval(timer)
})
</script>

<style scoped>
.home {
  padding: 20px;
}

.stat-item {
  display: flex;
  align-items: center;
  padding: 10px;
}

.stat-info {
  margin-left: 20px;
}

.stat-value {
  font-size: 28px;
  font-weight: bold;
  color: #303133;
}

.stat-label {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}

.welcome-card {
  margin-top: 20px;
}

.card-header {
  font-size: 16px;
  font-weight: bold;
}

.welcome-content h3 {
  margin: 0 0 16px;
  color: #303133;
}

.welcome-content p {
  color: #606266;
  margin: 8px 0;
}

.tips {
  color: #409EFF !important;
}
</style>
