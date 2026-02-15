import { useApiQuery } from './useApiQuery'
import type { OrgChartNode, OrgChartUser } from '../types/org-chart'

export const useOrgChart = (userId?: number, enabled = true) =>
  useApiQuery<OrgChartNode>(
    ['org-chart', userId ?? 'me'],
    userId ? `/api/v1/users/org-chart/${userId}` : '/api/v1/users/org-chart',
    undefined,
    enabled
  )

export const useOrgChartSearch = (query?: string, enabled = true) =>
  useApiQuery<OrgChartUser[]>(
    ['org-chart', 'search', query ?? ''],
    '/api/v1/users/org-search',
    query ? { query } : undefined,
    enabled && Boolean(query)
  )