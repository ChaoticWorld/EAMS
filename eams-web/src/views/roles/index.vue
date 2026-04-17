<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import type { FormInstance, FormRules } from 'element-plus'
import { roleApi, type Role, type CreateRoleRequest } from '@/api/role'

// 搜索条件
const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined
})

// 表格数据
const loading = ref(false)
const roleList = ref<Role[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('新增角色')
const formRef = ref<FormInstance>()
const form = ref<CreateRoleRequest>({
  roleName: '',
  roleCode: '',
  description: '',
  status: 1,
  sortOrder: 0
})
const editingId = ref<number | null>(null)

const rules: FormRules = {
  roleName: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
  roleCode: [{ required: true, message: '请输入角色编码', trigger: 'blur' }]
}

// 权限对话框
const permissionDialogVisible = ref(false)
const currentRoleId = ref<number>(0)
const permissionTreeRef = ref()
const permissionTreeData = ref([
  {
    id: 1,
    label: '系统管理',
    children: [
      { id: 11, label: '用户管理' },
      { id: 12, label: '角色管理' },
      { id: 13, label: '权限管理' },
      { id: 14, label: '消息管理' }
    ]
  },
  {
    id: 2,
    label: '资产管理',
    children: [
      { id: 21, label: '资产列表' },
      { id: 22, label: '资产分类' }
    ]
  }
])

// 加载数据
const loadData = async () => {
  loading.value = true
  try {
    const res = await roleApi.getRoles({
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword || undefined,
      status: searchForm.value.status
    })
    roleList.value = res.items
    total.value = res.total
  } catch (error: any) {
    ElMessage.error(error || '加载失败')
  } finally {
    loading.value = false
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
  dialogTitle.value = '新增角色'
  editingId.value = null
  form.value = {
    roleName: '',
    roleCode: '',
    description: '',
    status: 1,
    sortOrder: 0
  }
  dialogVisible.value = true
}

// 编辑
const handleEdit = (row: Role) => {
  dialogTitle.value = '编辑角色'
  editingId.value = row.id
  form.value = {
    roleName: row.roleName,
    roleCode: row.roleCode,
    description: row.description,
    status: row.status,
    sortOrder: row.sortOrder
  }
  dialogVisible.value = true
}

// 删除
const handleDelete = async (row: Role) => {
  try {
    await ElMessageBox.confirm(`确定删除角色 "${row.roleName}" 吗？`, '提示', {
      type: 'warning'
    })
    await roleApi.deleteRole(row.id)
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
      await roleApi.updateRole(editingId.value, {
        roleName: form.value.roleName,
        description: form.value.description,
        status: form.value.status,
        sortOrder: form.value.sortOrder
      })
      ElMessage.success('更新成功')
    } else {
      // 新增
      await roleApi.createRole(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '保存失败')
  }
}

// 分配权限
const handlePermission = (row: Role) => {
  currentRoleId.value = row.id
  permissionDialogVisible.value = true
  // TODO: 加载该角色的权限
}

// 保存权限
const handleSavePermission = async () => {
  try {
    const checkedKeys = permissionTreeRef.value?.getCheckedKeys() || []
    await roleApi.assignPermissions(currentRoleId.value, checkedKeys)
    ElMessage.success('权限分配成功')
    permissionDialogVisible.value = false
  } catch (error: any) {
    ElMessage.error(error || '权限分配失败')
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

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="role-management">
    <!-- 搜索栏 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="searchForm" inline>
        <el-form-item label="角色名称">
          <el-input v-model="searchForm.keyword" placeholder="请输入角色名称" clearable />
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
          <span>角色列表</span>
          <el-button type="primary" :icon="Plus" @click="handleAdd">新增角色</el-button>
        </div>
      </template>

      <!-- 表格 -->
      <el-table v-loading="loading" :data="roleList" border stripe>
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="roleName" label="角色名称" min-width="120" />
        <el-table-column prop="roleCode" label="角色编码" min-width="120" />
        <el-table-column prop="description" label="描述" min-width="180" show-overflow-tooltip />
        <el-table-column prop="sortOrder" label="排序" width="80" align="center" />
        <el-table-column prop="status" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="160" />
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link :icon="Edit" @click="handleEdit(row)">编辑</el-button>
            <el-button type="warning" link @click="handlePermission(row)">权限</el-button>
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
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="角色名称" prop="roleName">
          <el-input v-model="form.roleName" placeholder="请输入角色名称" />
        </el-form-item>
        <el-form-item label="角色编码" prop="roleCode">
          <el-input v-model="form.roleCode" placeholder="请输入角色编码" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入描述" />
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
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">确定</el-button>
      </template>
    </el-dialog>

    <!-- 权限分配对话框 -->
    <el-dialog v-model="permissionDialogVisible" title="分配权限" width="400px">
      <el-tree
        ref="permissionTreeRef"
        :data="permissionTreeData"
        show-checkbox
        default-expand-all
        node-key="id"
      />
      <template #footer>
        <el-button @click="permissionDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSavePermission">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.role-management {
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

.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
</style>
