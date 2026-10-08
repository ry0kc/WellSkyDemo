<script setup lang="ts">
import { ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { getReferralStatus, scheduleVisit } from '@/api'
import type { Referral } from '@/types'

// The router passes the :id route param in as a prop.
const props = defineProps<{ id: string }>()

// Page-only state stays local; Pinia is for state shared across views.
const referral = ref<Referral | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const visitDate = ref('')
const saving = ref(false)
const confirmation = ref<string | null>(null)

async function load() {
  loading.value = true
  error.value = null
  try {
    referral.value = await getReferralStatus(props.id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Lookup failed'
  } finally {
    loading.value = false
  }
}

// Reload if the route changes to another patient while this view stays mounted.
watch(() => props.id, load, { immediate: true })

async function submitVisit() {
  if (!visitDate.value) return
  saving.value = true
  confirmation.value = null
  error.value = null
  try {
    // datetime-local has no time zone; toISOString converts it to UTC,
    // which is what the API and Postgres timestamptz expect.
    const id = await scheduleVisit(props.id, new Date(visitDate.value).toISOString())
    confirmation.value = `Visit scheduled (ID ${id})`
    visitDate.value = ''
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Scheduling failed'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <main>
    <RouterLink to="/">Back to search</RouterLink>
    <h1>Patient {{ id }}</h1>

    <p v-if="loading">Loading...</p>
    <p v-else-if="error">{{ error }}</p>
    <section v-else-if="referral">
      <h2>Referral</h2>
      <p><strong>Provider:</strong> {{ referral.provider }}</p>
      <p><strong>Next step:</strong> {{ referral.nextStep }}</p>
      <p><strong>Due:</strong> {{ new Date(referral.dueDate).toLocaleDateString() }}</p>
    </section>
    <p v-else>No referral on file.</p>

    <h2>Schedule a visit</h2>
    <form @submit.prevent="submitVisit">
      <input type="datetime-local" v-model="visitDate" required />
      <button type="submit" :disabled="saving">Schedule</button>
    </form>
    <p v-if="confirmation">{{ confirmation }}</p>
  </main>
</template>