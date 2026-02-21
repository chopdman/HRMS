import { useApiQuery } from '../useApiQuery'
import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import type { HrEmailConfig } from '../../types/referrals'

export const useDefaultHrEmail = () =>
  useApiQuery<HrEmailConfig>(['referral-config', 'default-hr-email'], '/api/v1/referrals/config/default-hr-email')

export const useAnjumHrEmail = () =>
  useApiQuery<HrEmailConfig>(['referral-config', 'anjum-hr-email'], '/api/v1/referrals/config/anjum-hr-email')

export const useUpdateDefaultHrEmail = () =>
  useMutation({
    mutationFn: async (email: string) => {
      const response = await apiClient.put<HrEmailConfig>('/api/v1/referrals/config/default-hr-email', { email })
      return response.data?.data ?? response.data
    }
  })

export const useUpdateAnjumHrEmail = () =>
  useMutation({
    mutationFn: async (email: string) => {
      const response = await apiClient.put<HrEmailConfig>('/api/v1/referrals/config/anjum-hr-email', { email })
      return response.data?.data ?? response.data
    }
  })