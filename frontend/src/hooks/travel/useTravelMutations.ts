import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import type { TravelAssigned } from '../../types/travel'

type TravelAssignmentInput = { employeeId: number }

export type CreateTravelPayload = {
  travelName: string
  destination: string
  purpose?: string
  startDate: string
  endDate: string
  createdById: number
  assignments: TravelAssignmentInput[]
}

export const useCreateTravel = () =>
  useMutation({
    mutationFn: async (payload: CreateTravelPayload) => {
      const response = await apiClient.post<TravelAssigned>('/api/v1/travels', payload)
      return response.data
    }
  })