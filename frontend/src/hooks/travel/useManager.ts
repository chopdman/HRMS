import { useApiQuery } from '../useApiQuery'
import type { TeamExpense, TeamMember } from '../../types/manager'

export const useTeamMembers = () =>
  useApiQuery<TeamMember[]>(['manager', 'team-members'], '/api/v1/manager/team-members')

export const useTeamExpenses = (filters?: { from?: string; to?: string }) =>
  useApiQuery<TeamExpense[]>(['manager', 'team-expenses', filters], '/api/v1/manager/team-expenses', filters)