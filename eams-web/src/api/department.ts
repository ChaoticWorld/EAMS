import api from './request'

export interface Department {
  id: number
  deptName: string
  deptCode: string
  parentId: number | null
  description: string
  status: number
  sortOrder: number
  leader: string
  phone: string
  email: string
  createdAt: string
  parentName: string | null
  employeeCount: number
  children?: Department[]
}

export interface DepartmentTree {
  id: number
  deptName: string
  parentId: number | null
  sortOrder: number
  children?: DepartmentTree[]
}

export interface DepartmentQuery {
  pageIndex?: number
  pageSize?: number
  keyword?: string
  status?: number
  parentId?: number
}

export interface CreateDepartmentRequest {
  deptName: string
  deptCode?: string
  parentId?: number
  description?: string
  status?: number
  sortOrder?: number
  leader?: string
  phone?: string
  email?: string
}

export interface UpdateDepartmentRequest {
  deptName?: string
  deptCode?: string
  parentId?: number
  description?: string
  status?: number
  sortOrder?: number
  leader?: string
  phone?: string
  email?: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
  totalPages: number
}

export const departmentApi = {
  // 获取部门列表
  getDepartments: (params?: DepartmentQuery) => 
    api.get<PagedResult<Department>>('/departments', { params }),
  
  // 获取部门树
  getDepartmentTree: () => 
    api.get<DepartmentTree[]>('/departments/tree'),
  
  // 获取所有部门
  getAllDepartments: () => 
    api.get<Department[]>('/departments/all'),
  
  // 获取单个部门
  getDepartmentById: (id: number) => 
    api.get<Department>(`/departments/${id}`),
  
  // 创建部门
  createDepartment: (data: CreateDepartmentRequest) => 
    api.post<Department>('/departments', data),
  
  // 更新部门
  updateDepartment: (id: number, data: UpdateDepartmentRequest) => 
    api.put<Department>(`/departments/${id}`, data),
  
  // 删除部门
  deleteDepartment: (id: number) => 
    api.delete(`/departments/${id}`)
}
