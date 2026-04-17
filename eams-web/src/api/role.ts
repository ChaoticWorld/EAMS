import api from './request'

export interface Role {
  id: number
  roleName: string
  roleCode: string
  description: string
  status: number
  sortOrder: number
  createdAt: string
  permissions?: Permission[]
}

export interface Permission {
  id: number
  permissionName: string
  permissionCode: string
}

export interface RoleQuery {
  pageIndex?: number
  pageSize?: number
  keyword?: string
  status?: number
}

export interface CreateRoleRequest {
  roleName: string
  roleCode: string
  description?: string
  status?: number
  sortOrder?: number
  permissionIds?: number[]
}

export interface UpdateRoleRequest {
  id: number
  roleName?: string
  description?: string
  status?: number
  sortOrder?: number
  permissionIds?: number[]
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
  totalPages: number
}

export const roleApi = {
  // 获取角色列表
  getRoles: (params?: RoleQuery) => 
    api.get<PagedResult<Role>>('/roles', { params }),
  
  // 获取单个角色
  getRoleById: (id: number) => 
    api.get<Role>(`/roles/${id}`),
  
  // 创建角色
  createRole: (data: CreateRoleRequest) => 
    api.post<Role>('/roles', data),
  
  // 更新角色
  updateRole: (id: number, data: Omit<UpdateRoleRequest, 'id'>) => 
    api.put<Role>(`/roles/${id}`, data),
  
  // 删除角色
  deleteRole: (id: number) => 
    api.delete(`/roles/${id}`),
  
  // 获取角色权限
  getRolePermissions: (id: number) => 
    api.get<Permission[]>(`/roles/${id}/permissions`),
  
  // 分配权限
  assignPermissions: (roleId: number, permissionIds: number[]) => 
    api.post(`/roles/${roleId}/permissions`, { permissionIds })
}
