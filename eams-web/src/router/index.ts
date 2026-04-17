import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/login/index.vue'),
      meta: { public: true }
    },
    {
      path: '/',
      component: () => import('@/views/dashboard/index.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'Home',
          component: () => import('@/views/dashboard/Home.vue')
        },
        {
          path: '/users',
          name: 'Users',
          component: () => import('@/views/users/index.vue')
        },
        {
          path: '/roles',
          name: 'Roles',
          component: () => import('@/views/roles/index.vue')
        },
        {
          path: '/permissions',
          name: 'Permissions',
          component: () => import('@/views/permissions/index.vue')
        },
        {
          path: '/messages',
          name: 'Messages',
          component: () => import('@/views/messages/index.vue')
        },
        {
          path: '/departments',
          name: 'Departments',
          component: () => import('@/views/departments/index.vue')
        },
        {
          path: '/employees',
          name: 'Employees',
          component: () => import('@/views/employees/index.vue')
        }
      ]
    }
  ]
})

// 路由守卫
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  authStore.init()

  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    next('/login')
  } else if (to.path === '/login' && authStore.isLoggedIn) {
    next('/')
  } else {
    next()
  }
})

export default router
