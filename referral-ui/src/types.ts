export interface Patient {
  id: string
  name: string
  status: string
}

export interface Referral {
  patientId: string
  provider: string
  nextStep: string
  dueDate: string
}

export type Urgency = 'overdue' | 'due_soon' | 'on_track' | 'no_due_date'

export interface ReferralSummary {
  patientId: string
  provider: string
  nextStep: string
  dueDate: string | null
  daysUntilDue: number | null
  urgency: Urgency
}