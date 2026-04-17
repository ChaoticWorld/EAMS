import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi, type UserInfo, type LoginRequest } from '@/api/auth'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref(localStorage.getItem('token') || '')
  const user = ref<UserInfo | null>(null)
  const loading = ref(false)

  // Getters
  const isLoggedIn = computed(() => !!token.value)
  const username = computed(() => user.value?.username || '')

  // Actions
  const login = async (data: LoginRequest) => {
    loading.value = true
    try {
      const result = await authApi.login(data)
      token.value = result.token
      user.value = result.user
      localStorage.setItem('token', result.token)
      localStorage.setItem('user', JSON.stringify(result.user))
      return result
    } finally {
      loading.value = false
    }
  }

  const logout = async () => {
    try {
      await authApi.logout()
    } finally {
      token.value = ''
      user.value = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }

  const fetchCurrentUser = async () => {
    const userData = await authApi.getCurrentUser()
    user.value = userData
    return userData
  }

  const init = () => {
    const savedUser = localStorage.getItem('user')
    if (savedUser) {
      try {
        user.value = JSON.parse(savedUser)
      } catch {
        user.value = null
      }
    }
  }

  return {
    token,
    user,
    loading,
    isLoggedIn,
    username,
    login,
    logout,
    fetchCurrentUser,
    init
  }
})
