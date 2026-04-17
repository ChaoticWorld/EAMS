import request from './request'
import { ElMessage } from 'element-plus'
import type { UploadFile } from 'element-plus'

/**
 * 导出数据为Excel
 * @param url 导出API地址
 * @param filename 文件名（不含扩展名）
 */
export async function exportToExcel(url: string, filename: string): Promise<void> {
  try {
    const response = await fetch(`/api${url}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    if (!response.ok) {
      throw new Error('导出失败')
    }
    
    const blob = await response.blob()
    const downloadUrl = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = downloadUrl
    link.download = `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(downloadUrl)
    
    ElMessage.success('导出成功')
  } catch (error: any) {
    ElMessage.error(error.message || '导出失败')
  }
}

/**
 * 导入Excel数据
 * @param url 导入API地址
 * @param file 上传的文件
 * @returns 导入结果
 */
export async function importFromExcel(url: string, file: UploadFile): Promise<any> {
  const formData = new FormData()
  formData.append('file', file.raw!)
  
  return request.post(url, formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}

/**
 * 下载导入模板
 * @param url 模板下载API地址
 * @param filename 文件名
 */
export async function downloadTemplate(url: string, filename: string): Promise<void> {
  try {
    const response = await fetch(`/api${url}`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    })
    
    if (!response.ok) {
      throw new Error('下载模板失败')
    }
    
    const blob = await response.blob()
    const downloadUrl = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = downloadUrl
    link.download = `${filename}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(downloadUrl)
  } catch (error: any) {
    ElMessage.error(error.message || '下载模板失败')
  }
}
