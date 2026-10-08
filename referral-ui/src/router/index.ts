import { createRouter, createWebHistory } from 'vue-router'
import PatientSearchView from '@/views/PatientSearchView.vue'
import PatientDetailView from '@/views/PatientDetailView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'search', component: PatientSearchView },
    { path: '/patients/:id', name: 'patient', component: PatientDetailView, props: true },
  ],
})

export default router
