import type { Patient } from './types'

export async function searchPatients(name: string, status: string): Promise<Patient[]> {
  const params = new URLSearchParams()
  if (name) params.set('name', name)
  if (status) params.set('status', status)

  const res = await fetch(`/api/referrals/search?${params}`)
  if (!res.ok) throw new Error(`Search failed (${res.status})`)
  return res.json()
}