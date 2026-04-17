import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 后端统一响应格式
interface ApiResponse<T> {
  code: number
  message: string
  data: T
  timestamp: number
}

// 请求拦截器 - 添加 token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// 响应拦截器 - 处理错误并提取 data
api.interceptors.response.use(
  (response) => {
    const result = response.data as ApiResponse<any>
    // 后端统一响应格式：{ code, message, data }
    if (result.code !== 0) {
      return Promise.reject(result.message || '请求失败')
    }
    return result.data
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    const message = error.response?.data?.message || error.message || '请求失败'
    return Promise.reject(message)
  }
)

export default api
