import api from './request'

export interface User {
  id: number
  username: string
  realName: string
  email?: string
  phone?: string
  status: number
  createdAt?: string
}

export interface UserQuery {
  page?: number
  pageSize?: number
  keyword?: string
  status?: number
}

export interface CreateUserRequest {
  username: string
  password: string
  realName: string
  email?: string
  phone?: string
  roleIds?: number[]
}

export const userApi = {
  getUsers: (params: UserQuery) => api.get('/users', { params }),
  getUser: (id: number) => api.get(`/users/${id}`),
  createUser: (data: CreateUserRequest) => api.post('/users', data),
  updateUser: (id: number, data: Partial<CreateUserRequest>) => api.put(`/users/${id}`, data),
  deleteUser: (id: number) => api.delete(`/users/${id}`),
  assignRoles: (id: number, roleIds: number[]) => api.post(`/users/${id}/roles`, roleIds)
}
