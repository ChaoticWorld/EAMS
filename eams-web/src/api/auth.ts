import api from './request'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResult {
  token: string
  tokenType: string
  expiresIn: number
  user: UserInfo
}

export interface UserInfo {
  id: number
  username: string
  realName: string
  avatar?: string
  status: number
}

export const authApi = {
  login: (data: LoginRequest) => api.post<LoginResult>('/auth/login', data),
  logout: () => api.post('/auth/logout'),
  getCurrentUser: () => api.get<UserInfo>('/auth/me')
}
