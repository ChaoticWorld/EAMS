<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete, Download, Upload } from '@element-plus/icons-vue'
import type { FormInstance, FormRules, UploadFile } from 'element-plus'
import { departmentApi, type Department, type DepartmentTree, type CreateDepartmentRequest } from '@/api/department'
import { exportToExcel, importFromExcel, downloadTemplate } from '@/api/excel'
import { formatDateTime } from '@/utils/format'

// 搜索条件
const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined
})

// 表格数据
const loading = ref(false)
const departmentList = ref<Department[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 部门树（用于选择父部门）
const departmentTree = ref<DepartmentTree[]>([])

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('新增部门')
const formRef = ref<FormInstance>()
const form = ref<CreateDepartmentRequest & { id?: number }>({
  deptName: '',
  deptCode: '',
  parentId: undefined,
  description: '',
  status: 1,
  sortOrder: 0,
  leader: '',
  phone: '',
  email: ''
})
const editingId = ref<number | null>(null)

const rules: FormRules = {
  deptName: [{ required: true, message: '请输入部门名称', trigger: 'blur' }]
}

// 加载数据
const loadData = async () => {
  loading.value = true
  try {
    const res = await departmentApi.getDepartments({
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword || undefined,
      status: searchForm.value.status
    })
    departmentList.value = res.items
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
  searchForm.value = { keyword: '', status: undefined }
  handleSearch()
}

// 新增
const handleAdd = () => {
  dialogTitle.value = '新增部门'
  editingId.value = null
  form.value = {
    deptName: '',
    deptCode: '',
    parentId: undefined,
    description: '',
    status: 1,
    sortOrder: 0,
    leader: '',
    phone: '',
    email: ''
  }
  dialogVisible.value = true
}

// 编辑
const handleEdit = (row: Department) => {
  dialogTitle.value = '编辑部门'
  editingId.value = row.id
  form.value = {
    deptName: row.deptName,
    deptCode: row.deptCode,
    parentId: row.parentId ?? undefined,
    description: row.description,
    status: row.status,
    sortOrder: row.sortOrder,
    leader: row.leader,
    phone: row.phone,
    email: row.email
  }
  dialogVisible.value = true
}

// 删除
const handleDelete = async (row: Department) => {
  try {
    await ElMessageBox.confirm(`确定删除部门 "${row.deptName}" 吗？`, '提示', {
      type: 'warning'
    })
    await departmentApi.deleteDepartment(row.id)
    ElMessage.success('删除成功')
    loadData()
    loadDepartmentTree()
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
      await departmentApi.updateDepartment(editingId.value, {
        deptName: form.value.deptName,
        deptCode: form.value.deptCode,
        parentId: form.value.parentId,
        description: form.value.description,
        status: form.value.status,
        sortOrder: form.value.sortOrder,
        leader: form.value.leader,
        phone: form.value.phone,
        email: form.value.email
      })
      ElMessage.success('更新成功')
    } else {
      // 新增
      await departmentApi.createDepartment(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
    loadDepartmentTree()
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

// 导出
const handleExport = () => {
  exportToExcel('/departments/export', '部门列表')
}

// 导入
const importLoading = ref(false)
const handleImport = async (options: { file: UploadFile }) => {
  importLoading.value = true
  try {
    const result = await importFromExcel('/departments/import', options.file)
    ElMessage.success(`导入完成，成功 ${result.successCount} 条`)
    loadData()
    loadDepartmentTree()
  } catch (error: any) {
    ElMessage.error(error || '导入失败')
  } finally {
    importLoading.value = false
  }
}

// 下载模板
const handleDownloadTemplate = () => {
  downloadTemplate('/departments/template', '部门导入模板')
}

onMounted(() => {
  loadData()
  loadDepartmentTree()
})
</script>

<template>
  <div class="department-management">
    <!-- 搜索栏 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="searchForm" inline>
        <el-form-item label="部门名称">
          <el-input v-model="searchForm.keyword" placeholder="请输入部门名称" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.status" placeholder="请选择状态" clearable style="width: 120px">
            <el-option label="启用" :value="1" />
            <el-option label="禁用" :value="0" />
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
          <span>部门列表</span>
          <div class="header-buttons">
            <el-button type="primary" :icon="Plus" @click="handleAdd">新增部门</el-button>
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
      <el-table v-loading="loading" :data="departmentList" border stripe>
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="deptName" label="部门名称" min-width="150" />
        <el-table-column prop="deptCode" label="部门编码" width="120" />
        <el-table-column prop="parentName" label="上级部门" width="120" />
        <el-table-column prop="leader" label="负责人" width="100" />
        <el-table-column prop="phone" label="联系电话" width="130" />
        <el-table-column prop="employeeCount" label="员工数" width="80" align="center" />
        <el-table-column prop="sortOrder" label="排序" width="80" align="center" />
        <el-table-column prop="status" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'">
              {{ row.status === 1 ? '启用' : '禁用' }}
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
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="部门名称" prop="deptName">
          <el-input v-model="form.deptName" placeholder="请输入部门名称" />
        </el-form-item>
        <el-form-item label="部门编码">
          <el-input v-model="form.deptCode" placeholder="请输入部门编码" />
        </el-form-item>
        <el-form-item label="上级部门">
          <el-tree-select
            v-model="form.parentId"
            :data="departmentTree"
            :props="{ label: 'deptName', value: 'id', children: 'children' }"
            placeholder="请选择上级部门"
            clearable
            check-strictly
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="负责人">
          <el-input v-model="form.leader" placeholder="请输入负责人" />
        </el-form-item>
        <el-form-item label="联系电话">
          <el-input v-model="form.phone" placeholder="请输入联系电话" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="form.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="form.status">
            <el-radio :label="1">启用</el-radio>
            <el-radio :label="0">禁用</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入描述" />
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
.department-management {
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
