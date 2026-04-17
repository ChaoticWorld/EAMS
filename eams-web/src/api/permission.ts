import api from './request'

export interface Permission {
  id: number
  permissionName: string
  permissionCode: string
  permissionType: string  // 'menu' | 'button' | 'api'
  parentId: number | null
  path: string
  component: string
  icon: string
  sortOrder: number
  status: number
  createdAt: string
  children?: Permission[]
}

export interface PermissionTree {
  id: number
  permissionName: string
  permissionCode: string
  permissionType: string
  parentId: number | null
  path: string
  component: string
  icon: string
  sortOrder: number
  status: number
  createdAt: string
  children?: PermissionTree[]
}

export interface PermissionQuery {
  pageIndex?: number
  pageSize?: number
  keyword?: string
  permissionType?: string
  status?: number
}

export interface CreatePermissionRequest {
  permissionName: string
  permissionCode: string
  permissionType: string
  parentId?: number | null
  path?: string
  component?: string
  icon?: string
  sortOrder?: number
  status?: number
}

export interface UpdatePermissionRequest {
  permissionName?: string
  path?: string
  component?: string
  icon?: string
  sortOrder?: number
  status?: number
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

export const permissionApi = {
  // 获取权限列表
  getPermissions: (params?: PermissionQuery) => 
    api.get<PagedResult<Permission>>('/permissions', { params }),
  
  // 获取权限树
  getPermissionTree: () => 
    api.get<PermissionTree[]>('/permissions/tree'),
  
  // 获取单个权限
  getPermissionById: (id: number) => 
    api.get<Permission>(`/permissions/${id}`),
  
  // 创建权限
  createPermission: (data: CreatePermissionRequest) => 
    api.post<Permission>('/permissions', data),
  
  // 更新权限
  updatePermission: (id: number, data: UpdatePermissionRequest) => 
    api.put<Permission>(`/permissions/${id}`, data),
  
  // 删除权限
  deletePermission: (id: number) => 
    api.delete(`/permissions/${id}`)
}
