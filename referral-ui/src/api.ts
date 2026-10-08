import type { Patient, Referral } from './types'

export async function searchPatients(name: string, status: string): Promise<Patient[]> {
  const params = new URLSearchParams()
  if (name) params.set('name', name)
  if (status) params.set('status', status)

  const res = await fetch(`/api/referrals/search?${params}`)
  if (!res.ok) throw new Error(`Search failed (${res.status})`)
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
  if (!res.ok) throw new Error(`Scheduling failed (${res.status})`)
  const data: { visitId: string } = await res.json()
  return data.visitId
}