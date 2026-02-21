import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import { useApiQuery } from '../useApiQuery'
import type {
  ReferralCreatePayload,
  ReferralResponse,
  ReferralStatusLog,
  ReferralStatusUpdatePayload,
  ReferralStatusUpdateRequest
} from '../../types/referrals'

export const useCreateReferral = (jobId?: number) =>
  useMutation({
    mutationFn: async (payload: ReferralCreatePayload) => {
      if (!jobId) {
        throw new Error('Job ID is required')
      }
      const formData = new FormData()
      formData.append('FriendName', payload.friendName)
      if (payload.friendEmail) {
        formData.append('FriendEmail', payload.friendEmail)
      }
      if (payload.note) {
        formData.append('Note', payload.note)
      }
      formData.append('CvFile', payload.cvFile)

      const response = await apiClient.post<ReferralResponse>(`/api/v1/referrals/${jobId}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data?.data ?? response.data
    }
  })

export const useReferralsByJob = (jobId?: number, enabled = false) =>
  useApiQuery<ReferralResponse[]>(
    ['referrals', 'job', jobId],
    jobId ? `/api/v1/referrals/job/${jobId}` : '/api/v1/referrals/job/0',
    undefined,
    Boolean(jobId) && enabled
  )

export const useReferralLogs = (referralId?: number, enabled = false) =>
  useApiQuery<ReferralStatusLog[]>(
    ['referrals', 'logs', referralId],
    referralId ? `/api/v1/referrals/${referralId}/logs` : '/api/v1/referrals/0/logs',
    undefined,
    Boolean(referralId) && enabled
  )

export const useUpdateReferralStatus = () =>
  useMutation({
    mutationFn: async (payload: ReferralStatusUpdateRequest) => {
      const { referralId, ...body } = payload
      const response = await apiClient.put<ReferralResponse>(`/api/v1/referrals/${referralId}/status`, body)
      return response.data?.data ?? response.data
    }
  })