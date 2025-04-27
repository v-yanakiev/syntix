import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import ChatView from '../views/ChatView.vue'
import AuthView from '../views/AuthView.vue'
import BillingView from '../views/BillingView.vue'
import { useAuthStore } from '@/stores/auth'
import { usePostHog } from '@/composables/posthog'
import CustomEnvironmentsView from '@/views/CustomEnvironmentsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      children: [
        {
          path: 'chat/:id?',
          name: 'chat',
          component: ChatView,
          meta: { requiresAuth: true }
        },
        {
          path: 'auth',
          name: 'auth',
          component: AuthView
        },
        {
          path: 'customEnvironments',
          name: 'customEnvironments',
          component: CustomEnvironmentsView,
          meta: { requiresAuth: true }
        },
        { path: 'billing', name: 'billing', component: BillingView, meta: { requiresAuth: true } }
      ]
    }
  ]
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth && !authStore.userId) {
    next({ name: 'auth' })
  } else {
    next()
  }
})
const { posthog } = usePostHog()
export default router
