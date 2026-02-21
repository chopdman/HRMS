import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import type { JobCreatePayload, JobOpeningDetail, JobUpdatePayload } from '../../types/referrals'

const toFormData = (payload: JobCreatePayload) => {
  const formData = new FormData()
  formData.append('JobTitle', payload.jobTitle)
  if (payload.department) {
    formData.append('Department', payload.department)
  }
  if (payload.location) {
    formData.append('Location', payload.location)
  }
  formData.append('JobType', payload.jobType)
  if (payload.experienceRequired) {
    formData.append('ExperienceRequired', payload.experienceRequired)
  }
  if (payload.jobSummary) {
    formData.append('JobSummary', payload.jobSummary)
  }
  if (payload.jobDescriptionPath) {
    formData.append('JobDescriptionPath', payload.jobDescriptionPath)
  }
  if (payload.jobDescriptionFile) {
    formData.append('JobDescriptionFile', payload.jobDescriptionFile)
  }
  if (payload.hrOwnerEmail) {
    formData.append('HrOwnerEmail', payload.hrOwnerEmail)
  }
  if (payload.cvReviewerEmails) {
    payload.cvReviewerEmails.forEach((email) => {
      formData.append('CvReviewerEmails', email)
    })
  }
  formData.append('IsActive', String(payload.isActive))
  return formData
}

export const useCreateJobOpening = () =>
  useMutation({
    mutationFn: async (payload: JobCreatePayload) => {
      const response = await apiClient.post<JobOpeningDetail>('/api/v1/job-openings', toFormData(payload), {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data?.data ?? response.data
    }
  })

export const useUpdateJobOpening = (jobId?: number) =>
  useMutation({
    mutationFn: async (payload: JobUpdatePayload) => {
      if (!jobId) {
        throw new Error('Job ID is required')
      }
      const response = await apiClient.put<JobOpeningDetail>(`/api/v1/job-openings/${jobId}`, payload)
      return response.data?.data ?? response.data
    }
  })