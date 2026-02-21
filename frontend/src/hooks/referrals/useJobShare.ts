import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import type { JobShareRequest, JobShareResponse } from '../../types/referrals'

export const useShareJob = (jobId?: number) =>
  useMutation({
    mutationFn: async (payload: JobShareRequest) => {
      if (!jobId) {
        throw new Error('Job ID is required')
      }
      const response = await apiClient.post<JobShareResponse>(`/api/v1/job-openings/${jobId}/share`, payload)
      return response.data?.data ?? response.data
    }
  })