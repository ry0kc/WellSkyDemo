import type { Patient, Referral } from './types'
import type { ReferralSummary } from './types'

// A 400 from the API is a ValidationProblemDetails: { errors: { field: [messages] } }.
// Flatten those messages so views can show them; otherwise fall back to the status.
async function errorFrom(res: Response, fallback: string): Promise<Error> {
  if (res.status === 400) {
    try {
      const body: { errors?: Record<string, string[]> } = await res.json()
      const messages = Object.values(body.errors ?? {}).flat()
      if (messages.length) return new Error(messages.join('; '))
    } catch {
      // Not JSON; use the fallback below.
    }
  }
  return new Error(`${fallback} (${res.status})`)
}

export async function searchPatients(name: string, status: string): Promise<Patient[]> {
  const params = new URLSearchParams()
  if (name) params.set('name', name)
  if (status) params.set('status', status)

  const res = await fetch(`/api/referrals/search?${params}`)
  if (!res.ok) throw await errorFrom(res, 'Search failed')
  return res.json()
}

export async function getReferralStatus(patientId: string): Promise<Referral | null> {
  const res = await fetch(`/api/referrals/${encodeURIComponent(patientId)}/status`)
  if (res.status === 404) return null
  if (!res.ok) throw new Error(`Lookup failed (${res.status})`)
  return res.json()
}

export async function scheduleVisit(patientId: string, visitDate: string): Promise<string> {
  const res = await fetch('/api/referrals/schedule-visit', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ patientId, visitDate }),
  })
  if (!res.ok) throw await errorFrom(res, 'Scheduling failed')
  const data: { visitId: string } = await res.json()
  return data.visitId
}

export async function getReferralSummary(patientId: string): Promise<ReferralSummary | null> {
  const res = await fetch(`/api/referrals/${encodeURIComponent(patientId)}/summary`)
  if (res.status === 404) return null
  if (!res.ok) throw new Error(`Lookup failed (${res.status})`)
  return res.json()
}