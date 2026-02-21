import { useApiQuery } from '../useApiQuery'
import type { JobOpeningDetail, JobOpeningList } from '../../types/referrals'

export const useJobOpenings = (activeOnly = true) =>
  useApiQuery<JobOpeningList[]>(['job-openings', activeOnly], '/api/v1/job-openings', { activeOnly })

export const useJobOpening = (jobId?: number) =>
  useApiQuery<JobOpeningDetail>(
    ['job-openings', jobId],
    jobId ? `/api/v1/job-openings/${jobId}` : '/api/v1/job-openings/0',
    undefined,
    Boolean(jobId)
  )