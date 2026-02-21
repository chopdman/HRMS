export type JobType = 'FullTime' | 'PartTime' | 'Internship'

export type JobOpeningList = {
  jobId: number
  jobTitle: string
  department?: string | null
  location?: string | null
  jobType: JobType
  experienceRequired?: string | null
  jobSummary?: string | null
  isActive: boolean
  hrOwnerEmail?: string | null
  cvReviewerEmails: string[]
}

export type JobOpeningDetail = JobOpeningList & {
  jobDescriptionPath?: string | null
  postedById?: number | null
  postedAt: string
  updatedAt: string
}

export type JobShareRequest = {
  recipientEmails: string[]
}

export type JobShareResponse = {
  jobId: number
  recipients: string[]
  sharedAt: string
}

export type ReferralCreatePayload = {
  friendName: string
  friendEmail?: string
  note?: string
  cvFile: File
}

export type ReferralResponse = {
  referralId: number
  jobId: number
  referredById: number
  friendName: string
  friendEmail?: string | null
  cvFilePath: string
  note?: string | null
  status: string
  hrRecipients?: string | null
  submittedAt: string
  statusUpdatedAt?: string | null
  statusUpdatedBy?: number | null
}

export type ReferralStatusLog = {
  referralStatusLogId: number
  referralId: number
  oldStatus?: string | null
  newStatus: string
  recipientsSnapshot?: string | null
  note?: string | null
  changedAt: string
  changedById?: number | null
}

export type ReferralStatusUpdatePayload = {
  status: string
  note?: string
}

export type ReferralStatusUpdateRequest = ReferralStatusUpdatePayload & {
  referralId: number
}

export type HrEmailConfig = {
  email?: string | null
}

export type JobCreatePayload = {
  jobTitle: string
  department?: string
  location?: string
  jobType: JobType
  experienceRequired?: string
  jobSummary?: string
  jobDescriptionPath?: string
  jobDescriptionFile?: File
  hrOwnerEmail?: string
  cvReviewerEmails?: string[]
  isActive: boolean
}

export type JobUpdatePayload = {
  jobTitle: string
  department?: string
  location?: string
  jobType: JobType
  experienceRequired?: string
  jobSummary?: string
  jobDescriptionPath?: string
  jobDescriptionFile?: File
  hrOwnerEmail?: string
  cvReviewerEmails?: string[]
  isActive: boolean
}