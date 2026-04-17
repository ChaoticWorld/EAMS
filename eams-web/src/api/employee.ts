import api from './request'

export interface Employee {
  id: number
  employeeNo: string
  realName: string
  gender: number
  idCard: string
  phone: string
  email: string
  deptId: number | null
  hireDate: string | null
  leaveDate: string | null
  status: number
  position: string
  jobTitle: string
  avatar: string
  remark: string
  createdAt: string
  modifiedAt: string | null
  deptName: string | null
  userId: number | null
}

export interface EmployeeQuery {
  pageIndex?: number
  pageSize?: number
  keyword?: string
  status?: number
  deptId?: number
  gender?: number
}

export interface CreateEmployeeRequest {
  employeeNo: string
  realName: string
  gender?: number
  idCard?: string
  phone?: string
  email?: string
  deptId?: number
  hireDate?: string
  status?: number
  position?: string
  jobTitle?: string
  remark?: string
}

export interface UpdateEmployeeRequest {
  employeeNo?: string
  realName?: string
  gender?: number
  idCard?: string
  phone?: string
  email?: string
  deptId?: number
  hireDate?: string
  leaveDate?: string
  status?: number
  position?: string
  jobTitle?: string
  remark?: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
  totalPages: number
}

export const employeeApi = {
  // 获取员工列表
  getEmployees: (params?: EmployeeQuery) => 
    api.get<PagedResult<Employee>>('/employees', { params }),
  
  // 获取部门下的员工
  getEmployeesByDept: (deptId: number) => 
    api.get<Employee[]>(`/employees/department/${deptId}`),
  
  // 获取单个员工
  getEmployeeById: (id: number) => 
    api.get<Employee>(`/employees/${id}`),
  
  // 创建员工
  createEmployee: (data: CreateEmployeeRequest) => 
    api.post<Employee>('/employees', data),
  
  // 更新员工
  updateEmployee: (id: number, data: UpdateEmployeeRequest) => 
    api.put<Employee>(`/employees/${id}`, data),
  
  // 删除员工
  deleteEmployee: (id: number) => 
    api.delete(`/employees/${id}`)
}
