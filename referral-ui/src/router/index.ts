import { createRouter, createWebHistory } from 'vue-router'
import PatientSearchView from '@/views/PatientSearchView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [{ path: '/', name: 'search', component: PatientSearchView }],
})

export default router
