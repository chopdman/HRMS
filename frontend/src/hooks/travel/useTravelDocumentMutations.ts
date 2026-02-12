import { useMutation } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import type { TravelDocument } from '../../types/document'

type UploadPayload = {
  assignId: number
  travelId: number
  employeeId?: number
  documentType: string
  file: File
}

export const useUploadTravelDocument = () =>
  useMutation({
    mutationFn: async ({ assignId, travelId, employeeId, documentType, file }: UploadPayload) => {
      const formData = new FormData()
      formData.append('assignId', String(assignId))
      formData.append('travelId', String(travelId))
      if (employeeId) {
        formData.append('employeeId', String(employeeId))
      }
      formData.append('documentType', documentType)
      formData.append('file', file)

      const response = await apiClient.post<TravelDocument>('/api/v1/travel-documents', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })

      return response.data
    }
  })