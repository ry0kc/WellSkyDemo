<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { RouterLink } from 'vue-router'
import { getReferralSummary, scheduleVisit } from '@/api'
import type { ReferralSummary } from '@/types'

// The router passes the :id route param in as a prop.
const props = defineProps<{ id: string }>()

// Page-only state stays local; Pinia is for state shared across views.
const referral = ref<ReferralSummary | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const visitDate = ref('')
const saving = ref(false)
const confirmation = ref<string | null>(null)

const urgencyLabel: Record<ReferralSummary['urgency'], string> = {
  overdue: 'Overdue',
  due_soon: 'Due soon',
  on_track: 'On track',
  no_due_date: 'No due date',
}

// Recalculates only when referral changes; cached otherwise.
const dueText = computed(() => {
  const d = referral.value?.daysUntilDue
  if (d == null) return ''
  if (d < 0) return `${-d} day${d === -1 ? '' : 's'} overdue`
  if (d === 0) return 'due today'
  return `due in ${d} day${d === 1 ? '' : 's'}`
})

async function load() {
  loading.value = true
  error.value = null
  try {
    referral.value = await getReferralSummary(props.id)
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
      <p>
        <strong>Due:</strong>
        {{ referral.dueDate ? new Date(referral.dueDate).toLocaleDateString() : 'Not set' }}
        <span v-if="dueText">({{ dueText }})</span>
        <span :class="['badge', referral.urgency]">{{ urgencyLabel[referral.urgency] }}</span>
      </p>
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

<style scoped>
.badge { margin-left: 0.5rem; padding: 0.1rem 0.5rem; border-radius: 999px; font-size: 0.85em; }
.overdue { background: #fde2e1; color: #9b1c1c; }
.due_soon { background: #fef3c7; color: #92400e; }
.on_track { background: #dcfce7; color: #166534; }
.no_due_date { background: #e5e7eb; color: #374151; }
</style>