<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Download, Upload } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { employeeApi, type Employee, type CreateEmployeeRequest } from '@/api/employee'
import { departmentApi, type DepartmentTree } from '@/api/department'
import { exportToExcel, importFromExcel, downloadTemplate } from '@/api/excel'
import { formatDateTime, formatDate } from '@/utils/format'

// 搜索条件
const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
  deptId: undefined as number | undefined,
  gender: undefined as number | undefined
})

// 表格数据
const loading = ref(false)
const employeeList = ref<Employee[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 部门树（用于选择部门）
const departmentTree = ref<DepartmentTree[]>([])

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('新增员工')
const formRef = ref<FormInstance>()
const form = ref<CreateEmployeeRequest & { id?: number; leaveDate?: string }>({
  employeeNo: '',
  realName: '',
  gender: 0,
  idCard: '',
  phone: '',
  email: '',
  deptId: undefined,
  hireDate: '',
  status: 1,
  position: '',
  jobTitle: '',
  remark: ''
})
const editingId = ref<number | null>(null)

const rules: FormRules = {
  employeeNo: [{ required: true, message: '请输入工号', trigger: 'blur' }],
  realName: [{ required: true, message: '请输入姓名', trigger: 'blur' }]
}

// 加载数据
const loadData = async () => {
  loading.value = true
  try {
    const res = await employeeApi.getEmployees({
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword || undefined,
      status: searchForm.value.status,
      deptId: searchForm.value.deptId,
      gender: searchForm.value.gender
    })
    employeeList.value = res.items
    total.value = res.total
  } catch (error: any) {
    ElMessage.error(error || '加载失败')
  } finally {
    loading.value = false
  }
}

// 加载部门树
const loadDepartmentTree = async () => {
  try {
    departmentTree.value = await departmentApi.getDepartmentTree()
  } catch (error: any) {
    console.error('加载部门树失败', error)
  }
}

// 搜索
const handleSearch = () => {
  currentPage.value = 1
  loadData()
}

// 重置
const handleReset = () => {
  searchForm.value = { keyword: '', status: undefined, deptId: undefined, gender: undefined }
  handleSearch()
}

// 新增
const handleAdd = () => {
  dialogTitle.value = '新增员工'
  editingId.value = null
  form.value = {
    employeeNo: '',
    realName: '',
    gender: 0,
    idCard: '',
    phone: '',
    email: '',
    deptId: undefined,
    hireDate: '',
    status: 1,
    position: '',
    jobTitle: '',
    remark: ''
  }
  dialogVisible.value = true
}

// 编辑
const handleEdit = (row: Employee) => {
  dialogTitle.value = '编辑员工'
  editingId.value = row.id
  form.value = {
    employeeNo: row.employeeNo,
    realName: row.realName,
    gender: row.gender,
    idCard: row.idCard,
    phone: row.phone,
    email: row.email,
    deptId: row.deptId ?? undefined,
    hireDate: row.hireDate || '',
    leaveDate: row.leaveDate || '',
    status: row.status,
    position: row.position,
    jobTitle: row.jobTitle,
    remark: row.remark
  }
  dialogVisible.value = true
}

// 删除
const handleDelete = async (row: Employee) => {
  try {
    await ElMessageBox.confirm(`确定删除员工 "${row.realName}" 吗？`, '提示', {
      type: 'warning'
    })
    await employeeApi.deleteEmployee(row.id)
    ElMessage.success('删除成功')
    loadData()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error || '删除失败')
    }
  }
}

// 保存
const handleSave = async () => {
  if (!formRef.value) return
  try {
    await formRef.value.validate()
    if (editingId.value) {
      // 编辑
      await employeeApi.updateEmployee(editingId.value, {
        employeeNo: form.value.employeeNo,
        realName: form.value.realName,
        gender: form.value.gender,
        idCard: form.value.idCard,
        phone: form.value.phone,
        email: form.value.email,
        deptId: form.value.deptId,
        hireDate: form.value.hireDate,
        leaveDate: form.value.leaveDate,
        status: form.value.status,
        position: form.value.position,
        jobTitle: form.value.jobTitle,
        remark: form.value.remark
      })
      ElMessage.success('更新成功')
    } else {
      // 新增
      await employeeApi.createEmployee(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '保存失败')
  }
}

// 分页
const handleSizeChange = (val: number) => {
  pageSize.value = val
  loadData()
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
  loadData()
}

// 性别显示
const getGenderText = (gender: number) => {
  return gender === 1 ? '男' : gender === 2 ? '女' : '未知'
}

// 导出
const handleExport = () => {
  exportToExcel('/employees/export', '员工列表')
}

// 导入
const importLoading = ref(false)
const handleImport = async (options: { file: UploadFile }) => {
  importLoading.value = true
  try {
    const result = await importFromExcel('/employees/import', options.file)
    ElMessage.success(`导入完成，成功 ${result.successCount} 条`)
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '导入失败')
  } finally {
    importLoading.value = false
  }
}

// 下载模板
const handleDownloadTemplate = () => {
  downloadTemplate('/employees/template', '员工导入模板')
}

onMounted(() => {
  loadData()
  loadDepartmentTree()
})
</script>

<template>
  <div class="employee-management">
    <!-- 搜索栏 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="searchForm" inline>
        <el-form-item label="姓名/工号">
          <el-input v-model="searchForm.keyword" placeholder="请输入姓名或工号" clearable />
        </el-form-item>
        <el-form-item label="部门">
          <el-tree-select
            v-model="searchForm.deptId"
            :data="departmentTree"
            :props="{ label: 'deptName', value: 'id', children: 'children' }"
            placeholder="请选择部门"
            clearable
            check-strictly
            style="width: 180px"
          />
        </el-form-item>
        <el-form-item label="性别">
          <el-select v-model="searchForm.gender" placeholder="请选择" clearable style="width: 100px">
            <el-option label="男" :value="1" />
            <el-option label="女" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.status" placeholder="请选择状态" clearable style="width: 100px">
            <el-option label="在职" :value="1" />
            <el-option label="离职" :value="0" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleSearch">查询</el-button>
          <el-button :icon="Refresh" @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 操作栏 -->
    <el-card class="table-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>员工列表</span>
          <div class="header-buttons">
            <el-button type="primary" :icon="Plus" @click="handleAdd">新增员工</el-button>
            <el-button type="success" :icon="Download" @click="handleExport">导出</el-button>
            <el-upload
              :show-file-list="false"
              :before-upload="() => false"
              :on-change="handleImport"
              accept=".xlsx,.xls"
            >
              <el-button type="warning" :icon="Upload" :loading="importLoading">导入</el-button>
            </el-upload>
            <el-button :icon="Download" @click="handleDownloadTemplate">下载模板</el-button>
          </div>
        </div>
      </template>

      <!-- 表格 -->
      <el-table v-loading="loading" :data="employeeList" border stripe>
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="employeeNo" label="工号" width="100" />
        <el-table-column prop="realName" label="姓名" width="100" />
        <el-table-column prop="gender" label="性别" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.gender === 1 ? 'primary' : row.gender === 2 ? 'danger' : 'info'" size="small">
              {{ getGenderText(row.gender) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="deptName" label="所属部门" min-width="120" />
        <el-table-column prop="position" label="职位" width="120" />
        <el-table-column prop="jobTitle" label="职称" width="120" />
        <el-table-column prop="phone" label="联系电话" width="130" />
        <el-table-column prop="email" label="邮箱" min-width="180" show-overflow-tooltip />
        <el-table-column prop="hireDate" label="入职日期" width="110" />
        <el-table-column prop="status" label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'">
              {{ row.status === 1 ? '在职' : '离职' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="170">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link :icon="Edit" @click="handleEdit(row)">编辑</el-button>
            <el-button type="danger" link :icon="Delete" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        class="pagination"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="650px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="工号" prop="employeeNo">
              <el-input v-model="form.employeeNo" placeholder="请输入工号" :disabled="!!editingId" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="姓名" prop="realName">
              <el-input v-model="form.realName" placeholder="请输入姓名" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="性别">
              <el-radio-group v-model="form.gender">
                <el-radio :label="0">未知</el-radio>
                <el-radio :label="1">男</el-radio>
                <el-radio :label="2">女</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="部门">
              <el-tree-select
                v-model="form.deptId"
                :data="departmentTree"
                :props="{ label: 'deptName', value: 'id', children: 'children' }"
                placeholder="请选择部门"
                clearable
                check-strictly
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="身份证">
              <el-input v-model="form.idCard" placeholder="请输入身份证号" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="联系电话">
              <el-input v-model="form.phone" placeholder="请输入联系电话" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="邮箱">
              <el-input v-model="form.email" placeholder="请输入邮箱" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="入职日期">
              <el-date-picker
                v-model="form.hireDate"
                type="date"
                placeholder="选择日期"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="职位">
              <el-input v-model="form.position" placeholder="请输入职位" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="职称">
              <el-input v-model="form.jobTitle" placeholder="请输入职称" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="状态">
              <el-radio-group v-model="form.status">
                <el-radio :label="1">在职</el-radio>
                <el-radio :label="0">离职</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="editingId">
            <el-form-item label="离职日期">
              <el-date-picker
                v-model="form.leaveDate"
                type="date"
                placeholder="选择日期"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.employee-management {
  padding: 20px;
}

.search-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-buttons {
  display: flex;
  gap: 10px;
}

.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
</style>
