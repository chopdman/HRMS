import { useApiQuery } from '../useApiQuery'
import type { TravelAssigned, TravelAssignment } from '../../types/travel'

export const useAssignedTravels = (employeeId?: number, enabled = true) =>
  useApiQuery<TravelAssigned[]>(
    ['travels', 'assigned', employeeId ?? 'self'],
    '/api/v1/travels/assignments',
    employeeId ? { employeeId } : undefined,
    enabled
  )

  //testing
export const useTravelAssignments = (employeeId?: number, enabled = true) =>
  useApiQuery<TravelAssignment[]>(
    ['travels', 'assignments', employeeId ?? 'self'],
    '/api/v1/travels/assignments',
    employeeId ? { employeeId } : undefined,
    enabled
  )