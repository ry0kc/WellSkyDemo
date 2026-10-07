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