<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { usePatientStore } from '@/stores/patients'
import { RouterLink } from 'vue-router'

const store = usePatientStore()
// storeToRefs keeps these reactive after destructuring.
const { results, loading, error } = storeToRefs(store)

const name = ref('')
const status = ref('')

function runSearch() {
  store.search(name.value, status.value)
}

onMounted(runSearch)
</script>

<template>
  <main>
    <h1>Patient search</h1>

    <form @submit.prevent="runSearch">
      <input v-model="name" placeholder="Name contains" />
      <select v-model="status">
        <option value="">Any status</option>
        <option value="in_care">In care</option>
        <option value="pending_admission">Pending admission</option>
        <option value="discharged">Discharged</option>
      </select>
      <button type="submit" :disabled="loading">Search</button>
    </form>

    <p v-if="error">{{ error }}</p>
    <p v-else-if="loading">Searching...</p>
    <p v-else-if="results.length === 0">No patients found.</p>
    <ul v-else>
      <li v-for="p in results" :key="p.id">
        <RouterLink :to="{ name: 'patient', params: { id: p.id } }">{{ p.name }}</RouterLink>
        <small>({{ p.status }})</small>
      </li>
    </ul>
  </main>
</template>