<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import type { FormInstance, FormRules } from 'element-plus'
import { permissionApi, type Permission, type PermissionTree, type CreatePermissionRequest, type UpdatePermissionRequest } from '@/api/permission'

// 搜索条件
const searchForm = ref({
  keyword: '',
  permissionType: undefined as string | undefined
})

// 表格数据
const loading = ref(false)
const permissionList = ref<Permission[]>([])
const permissionTree = ref<PermissionTree[]>([])

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('新增权限')
const formRef = ref<FormInstance>()
const form = ref<CreatePermissionRequest>({
  permissionName: '',
  permissionCode: '',
  permissionType: 'menu',
  parentId: null,
  path: '',
  component: '',
  icon: '',
  sortOrder: 0,
  status: 1
})
const editingId = ref<number | null>(null)

const rules: FormRules = {
  permissionName: [{ required: true, message: '请输入权限名称', trigger: 'blur' }],
  permissionCode: [{ required: true, message: '请输入权限编码', trigger: 'blur' }],
  permissionType: [{ required: true, message: '请选择权限类型', trigger: 'change' }]
}

// 权限类型选项
const typeOptions = [
  { label: '目录', value: 'catalog' },
  { label: '菜单', value: 'menu' },
  { label: '按钮', value: 'button' }
]

// 图标选项
const iconOptions = [
  'HomeFilled',
  'UserFilled',
  'User',
  'Lock',
  'Message',
  'Setting',
  'Menu',
  'Document',
  'Folder',
  'FolderOpened'
]

// 父级权限选项（目录类型）
const parentOptions = computed(() => {
  return permissionTree.value.filter(p => p.permissionType === 'catalog')
})

// 加载数据
const loadData = async () => {
  loading.value = true
  try {
    // 获取权限树
    const tree = await permissionApi.getPermissionTree()
    permissionTree.value = tree
    
    // 转换为扁平列表用于表格显示
    const flatten = (items: PermissionTree[], result: Permission[] = []): Permission[] => {
      for (const item of items) {
        const { children, ...rest } = item
        result.push(rest as Permission)
        if (children && children.length > 0) {
          flatten(children, result)
        }
      }
      return result
    }
    
    // 构建带children的树形结构用于表格
    const buildTree = (items: PermissionTree[]): Permission[] => {
      return items.map(item => ({
        ...item,
        children: item.children && item.children.length > 0 ? buildTree(item.children) : undefined
      }))
    }
    
    permissionList.value = buildTree(tree)
  } catch (error: any) {
    ElMessage.error(error || '加载失败')
  } finally {
    loading.value = false
  }
}

// 搜索
const handleSearch = () => {
  loadData()
}

// 重置
const handleReset = () => {
  searchForm.value = { keyword: '', permissionType: undefined }
  handleSearch()
}

// 新增
const handleAdd = () => {
  dialogTitle.value = '新增权限'
  editingId.value = null
  form.value = {
    permissionName: '',
    permissionCode: '',
    permissionType: 'menu',
    parentId: null,
    path: '',
    component: '',
    icon: '',
    sortOrder: 0,
    status: 1
  }
  dialogVisible.value = true
}

// 编辑
const handleEdit = async (row: Permission) => {
  dialogTitle.value = '编辑权限'
  editingId.value = row.id
  form.value = {
    permissionName: row.permissionName,
    permissionCode: row.permissionCode,
    permissionType: row.permissionType,
    parentId: row.parentId,
    path: row.path,
    component: row.component,
    icon: row.icon,
    sortOrder: row.sortOrder,
    status: row.status
  }
  dialogVisible.value = true
}

// 删除
const handleDelete = async (row: Permission) => {
  try {
    await ElMessageBox.confirm(`确定删除权限 "${row.permissionName}" 吗？`, '提示', {
      type: 'warning'
    })
    await permissionApi.deletePermission(row.id)
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
      const updateData: UpdatePermissionRequest = {
        permissionName: form.value.permissionName,
        path: form.value.path,
        component: form.value.component,
        icon: form.value.icon,
        sortOrder: form.value.sortOrder,
        status: form.value.status
      }
      await permissionApi.updatePermission(editingId.value, updateData)
      ElMessage.success('更新成功')
    } else {
      // 新增
      await permissionApi.createPermission(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (error: any) {
    ElMessage.error(error || '保存失败')
  }
}

// 获取类型标签
const getTypeLabel = (type: string) => {
  const map: Record<string, string> = { catalog: '目录', menu: '菜单', button: '按钮' }
  return map[type] || type
}

// 获取类型标签样式
const getTypeTagType = (type: string) => {
  const map: Record<string, any> = { catalog: 'primary', menu: 'success', button: 'warning' }
  return map[type] || 'info'
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="permission-management">
    <!-- 搜索栏 -->
    <el-card class="search-card" shadow="never">
      <el-form :model="searchForm" inline>
        <el-form-item label="权限名称">
          <el-input v-model="searchForm.keyword" placeholder="请输入权限名称" clearable />
        </el-form-item>
        <el-form-item label="权限类型">
          <el-select v-model="searchForm.permissionType" placeholder="请选择类型" clearable style="width: 120px">
            <el-option label="目录" value="catalog" />
            <el-option label="菜单" value="menu" />
            <el-option label="按钮" value="button" />
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
          <span>权限列表</span>
          <el-button type="primary" :icon="Plus" @click="handleAdd">新增权限</el-button>
        </div>
      </template>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="permissionList"
        border
        stripe
        row-key="id"
        default-expand-all
        :tree-props="{ children: 'children' }"
      >
        <el-table-column prop="permissionName" label="权限名称" min-width="150" />
        <el-table-column prop="permissionCode" label="权限编码" min-width="180" />
        <el-table-column prop="permissionType" label="类型" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="getTypeTagType(row.permissionType)">
              {{ getTypeLabel(row.permissionType) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="path" label="路由路径" min-width="150" />
        <el-table-column prop="icon" label="图标" width="100" align="center">
          <template #default="{ row }">
            <el-icon v-if="row.icon"><component :is="row.icon" /></el-icon>
          </template>
        </el-table-column>
        <el-table-column prop="sortOrder" label="排序" width="80" align="center" />
        <el-table-column prop="status" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="160" />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link :icon="Edit" @click="handleEdit(row)">编辑</el-button>
            <el-button type="danger" link :icon="Delete" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="权限类型" prop="permissionType">
          <el-radio-group v-model="form.permissionType" :disabled="!!editingId">
            <el-radio label="catalog">目录</el-radio>
            <el-radio label="menu">菜单</el-radio>
            <el-radio label="button">按钮</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="上级权限" v-if="form.permissionType !== 'catalog'">
          <el-select v-model="form.parentId" placeholder="请选择上级权限" clearable style="width: 100%">
            <el-option
              v-for="item in parentOptions"
              :key="item.id"
              :label="item.permissionName"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="权限名称" prop="permissionName">
          <el-input v-model="form.permissionName" placeholder="请输入权限名称" />
        </el-form-item>
        <el-form-item label="权限编码" prop="permissionCode">
          <el-input v-model="form.permissionCode" placeholder="请输入权限编码" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item label="路由路径" v-if="form.permissionType !== 'button'">
          <el-input v-model="form.path" placeholder="请输入路由路径，如：/users" />
        </el-form-item>
        <el-form-item label="组件路径" v-if="form.permissionType === 'menu'">
          <el-input v-model="form.component" placeholder="请输入组件路径，如：views/users/index.vue" />
        </el-form-item>
        <el-form-item label="图标" v-if="form.permissionType !== 'button'">
          <el-select v-model="form.icon" placeholder="请选择图标" clearable style="width: 100%">
            <el-option
              v-for="icon in iconOptions"
              :key="icon"
              :label="icon"
              :value="icon"
            >
              <el-icon style="margin-right: 8px"><component :is="icon" /></el-icon>
              {{ icon }}
            </el-option>
          </el-select>
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
  </div>
</template>

<style scoped>
.permission-management {
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
</style>
