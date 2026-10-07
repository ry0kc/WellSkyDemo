import { defineStore } from 'pinia'
import { ref } from 'vue'
import { searchPatients } from '@/api'
import type { Patient } from '@/types'

export const usePatientStore = defineStore('patients', () => {
  const results = ref<Patient[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function search(name: string, status: string) {
    loading.value = true
    error.value = null
    try {
      results.value = await searchPatients(name, status)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Search failed'
    } finally {
      loading.value = false
    }
  }

  return { results, loading, error, search }
})