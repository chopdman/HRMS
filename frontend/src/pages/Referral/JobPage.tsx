import { useEffect, useState } from 'react'
import { Header } from '../../components/Header'
import { Card } from '../../components/ui/Card'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Modal } from '../../components/ui/Modal'
import { EmptyState } from '../../components/ui/EmptyState'
import { Spinner } from '../../components/ui/Spinner'
import { Select } from '../../components/ui/Select'
import { useAuth } from '../../hooks/useAuth'
import { useJobOpening, useJobOpenings } from '../../hooks/referrals/useJobOpenings'
import { useCreateJobOpening, useUpdateJobOpening } from '../../hooks/referrals/useJobMutations'
import { useShareJob } from '../../hooks/referrals/useJobShare'
import {
  useCreateReferral,
  useReferralLogs,
  useReferralsByJob,
  useUpdateReferralStatus
} from '../../hooks/referrals/useReferrals'
import {
  useAnjumHrEmail,
  useDefaultHrEmail,
  useUpdateAnjumHrEmail,
  useUpdateDefaultHrEmail
} from '../../hooks/referrals/useReferralConfig'
import type {
  JobCreatePayload,
  JobOpeningList,
  JobUpdatePayload,
  ReferralResponse,
  ReferralStatusLog
} from '../../types/referrals'

const parseEmails = (value: string) =>
  value
    .split(',')
    .map((email) => email.trim())
    .filter(Boolean)

export const JobsPage = () => {
  const { role } = useAuth()
  const isHr = role === 'HR'
  const [activeJob, setActiveJob] = useState<JobOpeningList | null>(null)
  const [shareRecipients, setShareRecipients] = useState('')
  const [referralFriendName, setReferralFriendName] = useState('')
  const [referralFriendEmail, setReferralFriendEmail] = useState('')
  const [referralNote, setReferralNote] = useState('')
  const [referralCv, setReferralCv] = useState<File | null>(null)
  const [shareOpen, setShareOpen] = useState(false)
  const [referOpen, setReferOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [referralsOpen, setReferralsOpen] = useState(false)
  const [editJobId, setEditJobId] = useState<number | null>(null)
  const [referralJob, setReferralJob] = useState<JobOpeningList | null>(null)
  const [selectedReferralId, setSelectedReferralId] = useState<number | null>(null)
  const [jobForm, setJobForm] = useState<JobCreatePayload>({
    jobTitle: '',
    department: '',
    location: '',
    jobType: 'FullTime',
    experienceRequired: '',
    jobSummary: '',
    jobDescriptionPath: '',
    hrOwnerEmail: '',
    cvReviewerEmails: [],
    isActive: true
  })
  const [reviewerEmails, setReviewerEmails] = useState('')
  const [editReviewerEmails, setEditReviewerEmails] = useState('')
  const [statusById, setStatusById] = useState<Record<number, string>>({})
  const [noteById, setNoteById] = useState<Record<number, string>>({})
  const [editForm, setEditForm] = useState<JobUpdatePayload>({
    jobTitle: '',
    department: '',
    location: '',
    jobType: 'FullTime',
    experienceRequired: '',
    jobSummary: '',
    jobDescriptionPath: '',
    hrOwnerEmail: '',
    cvReviewerEmails: [],
    isActive: true
  })

  const jobsQuery = useJobOpenings(true)
  const editJobQuery = useJobOpening(editJobId ?? undefined)
  const createJob = useCreateJobOpening()
  const updateJob = useUpdateJobOpening(editJobId ?? undefined)
  const shareJob = useShareJob(activeJob?.jobId)
  const createReferral = useCreateReferral(activeJob?.jobId)
  const updateReferralStatus = useUpdateReferralStatus()
  const referralsQuery = useReferralsByJob(referralJob?.jobId, referralsOpen && isHr)
  const referralLogsQuery = useReferralLogs(selectedReferralId ?? undefined, Boolean(selectedReferralId) && referralsOpen)

  const defaultHrQuery = useDefaultHrEmail()
  const anjumHrQuery = useAnjumHrEmail()
  const updateDefaultHr = useUpdateDefaultHrEmail()
  const updateAnjumHr = useUpdateAnjumHrEmail()
  const [defaultHrEmail, setDefaultHrEmail] = useState('')
  const [anjumHrEmail, setAnjumHrEmail] = useState('')

  const jobs = jobsQuery.data ?? []
  const referrals = referralsQuery.data ?? []
  const referralLogs = referralLogsQuery.data ?? []

  const statusOptions = ['New', 'InReview', 'Shortlisted', 'Rejected', 'Hired']

  useEffect(() => {
    if (!defaultHrQuery.data) {
      return
    }
    setDefaultHrEmail(defaultHrQuery.data.email ?? '')
  }, [defaultHrQuery.data])

  useEffect(() => {
    if (!anjumHrQuery.data) {
      return
    }
    setAnjumHrEmail(anjumHrQuery.data.email ?? '')
  }, [anjumHrQuery.data])

  useEffect(() => {
    if (!editJobQuery.data) {
      return
    }

    const detail = editJobQuery.data
    setEditReviewerEmails(detail.cvReviewerEmails?.join(', ') ?? '')
    setEditForm({
      jobTitle: detail.jobTitle,
      department: detail.department ?? '',
      location: detail.location ?? '',
      jobType: detail.jobType,
      experienceRequired: detail.experienceRequired ?? '',
      jobSummary: detail.jobSummary ?? '',
      jobDescriptionPath: detail.jobDescriptionPath ?? '',
      hrOwnerEmail: detail.hrOwnerEmail ?? '',
      cvReviewerEmails: detail.cvReviewerEmails ?? [],
      isActive: detail.isActive
    })
  }, [editJobQuery.data])

  useEffect(() => {
    if (!referrals.length) {
      return
    }

    setStatusById((prev) => {
      const updated = { ...prev }
      referrals.forEach((referral) => {
        if (!updated[referral.referralId]) {
          updated[referral.referralId] = referral.status
        }
      })
      return updated
    })
  }, [referrals])

  const handleJobSubmit = async () => {
    const payload: JobCreatePayload = {
      ...jobForm,
      jobType: jobForm.jobType,
      cvReviewerEmails: parseEmails(reviewerEmails),
      jobDescriptionFile: jobForm.jobDescriptionFile
    }
    await createJob.mutateAsync(payload)
    setJobForm({
      jobTitle: '',
      department: '',
      location: '',
      jobType: 'FullTime',
      experienceRequired: '',
      jobSummary: '',
      jobDescriptionPath: '',
      hrOwnerEmail: '',
      cvReviewerEmails: [],
      isActive: true
    })
    setReviewerEmails('')
    await jobsQuery.refetch()
  }

  const handleShare = async () => {
    if (!activeJob) {
      return
    }
    await shareJob.mutateAsync({ recipientEmails: parseEmails(shareRecipients) })
    setShareRecipients('')
    setShareOpen(false)
  }

  const handleRefer = async () => {
    if (!activeJob || !referralCv) {
      return
    }
    await createReferral.mutateAsync({
      friendName: referralFriendName,
      friendEmail: referralFriendEmail || undefined,
      note: referralNote || undefined,
      cvFile: referralCv
    })
    setReferralFriendName('')
    setReferralFriendEmail('')
    setReferralNote('')
    setReferralCv(null)
    setReferOpen(false)
  }

  const handleUpdateDefaultHr = async () => {
    await updateDefaultHr.mutateAsync(defaultHrEmail)
    await defaultHrQuery.refetch()
  }

  const handleUpdateAnjumHr = async () => {
    await updateAnjumHr.mutateAsync(anjumHrEmail)
    await anjumHrQuery.refetch()
  }

  const handleEditSubmit = async () => {
    if (!editJobId) {
      return
    }
    const payload: JobUpdatePayload = {
      ...editForm,
      cvReviewerEmails: parseEmails(editReviewerEmails)
    }
    await updateJob.mutateAsync(payload)
    setEditOpen(false)
    await jobsQuery.refetch()
  }

  const handleReferralStatusUpdate = async (referral: ReferralResponse) => {
    const status = statusById[referral.referralId] ?? referral.status
    const note = noteById[referral.referralId]
    await updateReferralStatus.mutateAsync({
      referralId: referral.referralId,
      status,
      note: note || undefined
    })
    await referralsQuery.refetch()
    if (selectedReferralId === referral.referralId) {
      await referralLogsQuery.refetch()
    }
  }

  return (
    <div className="space-y-6">
      <Header
        title="Job Openings"
        description="Browse active openings, share them with candidates, or refer friends to HR."
      />

      {isHr ? (
        <Card className="space-y-4">
          <h3 className="text-lg font-semibold">Create Job Opening</h3>
          <div className="grid gap-4 md:grid-cols-2">
            <Input
              label="Job title"
              value={jobForm.jobTitle}
              onChange={(event) => setJobForm((prev) => ({ ...prev, jobTitle: event.target.value }))}
            />
            <Input
              label="Department"
              value={jobForm.department}
              onChange={(event) => setJobForm((prev) => ({ ...prev, department: event.target.value }))}
            />
            <Input
              label="Location"
              value={jobForm.location}
              onChange={(event) => setJobForm((prev) => ({ ...prev, location: event.target.value }))}
            />
            <Select
              label="Job type"
              value={jobForm.jobType}
              onChange={(event) =>
                setJobForm((prev) => ({
                  ...prev,
                  jobType: event.target.value as JobCreatePayload['jobType']
                }))
              }
            >
              <option value="FullTime">Full time</option>
              <option value="PartTime">Part time</option>
              <option value="Internship">Internship</option>
            </Select>
            <Input
              label="Experience required"
              value={jobForm.experienceRequired}
              onChange={(event) => setJobForm((prev) => ({ ...prev, experienceRequired: event.target.value }))}
            />
            <Input
              label="HR owner email"
              value={jobForm.hrOwnerEmail}
              onChange={(event) => setJobForm((prev) => ({ ...prev, hrOwnerEmail: event.target.value }))}
            />
            <Input
              label="CV reviewer emails (comma-separated)"
              value={reviewerEmails}
              onChange={(event) => setReviewerEmails(event.target.value)}
            />
            <Input
              label="JD file path (optional if uploading)"
              value={jobForm.jobDescriptionPath}
              onChange={(event) => setJobForm((prev) => ({ ...prev, jobDescriptionPath: event.target.value }))}
            />
            <label className="block space-y-2 text-sm">
              <span className="font-medium text-(--color-dark)">JD file upload</span>
              <input
                type="file"
                className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
                onChange={(event) =>
                  setJobForm((prev) => ({
                    ...prev,
                    jobDescriptionFile: event.target.files?.[0] ?? undefined
                  }))
                }
              />
            </label>
          </div>
          <label className="block space-y-2 text-sm">
            <span className="font-medium text-(--color-dark)">Job summary</span>
            <textarea
              className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
              rows={3}
              value={jobForm.jobSummary}
              onChange={(event) => setJobForm((prev) => ({ ...prev, jobSummary: event.target.value }))}
            />
          </label>
          <div className="flex items-center gap-3">
            <label className="flex items-center gap-2 text-sm font-medium">
              <input
                type="checkbox"
                checked={jobForm.isActive}
                onChange={(event) => setJobForm((prev) => ({ ...prev, isActive: event.target.checked }))}
              />
              Active opening
            </label>
            <Button onClick={handleJobSubmit} disabled={createJob.isPending}>
              {createJob.isPending ? 'Saving...' : 'Create job'}
            </Button>
          </div>
        </Card>
      ) : null}

      {isHr ? (
        <Card className="space-y-4">
          <h3 className="text-lg font-semibold">Referral Email Settings</h3>
          <div className="grid gap-4 md:grid-cols-2">
            <Input
              label="Default HR email"
              value={defaultHrEmail}
              onChange={(event) => setDefaultHrEmail(event.target.value)}
            />
            <Input
              label="Anjum HR email"
              value={anjumHrEmail}
              onChange={(event) => setAnjumHrEmail(event.target.value)}
            />
          </div>
          <div className="flex gap-3">
            <Button onClick={handleUpdateDefaultHr} disabled={updateDefaultHr.isPending}>
              Save default HR
            </Button>
            <Button onClick={handleUpdateAnjumHr} disabled={updateAnjumHr.isPending}>
              Save Anjum HR
            </Button>
          </div>
        </Card>
      ) : null}

      <Card>
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold">Active openings</h3>
          {jobsQuery.isLoading ? <Spinner /> : null}
        </div>
        {jobsQuery.isError ? (
          <p className="mt-3 text-sm text-red-500">Failed to load job openings.</p>
        ) : null}
        {jobs.length === 0 && !jobsQuery.isLoading ? (
          <EmptyState
            title="No openings"
            description="HR hasn't published any active job openings yet."
          />
        ) : null}
        <div className="mt-4 grid gap-4">
          {jobs.map((job) => (
            <div key={job.jobId} className="rounded-xl border border-slate-200 bg-white p-4">
              <div className="flex flex-col gap-2 md:flex-row md:items-start md:justify-between">
                <div>
                  <h4 className="text-lg font-semibold text-slate-900">{job.jobTitle}</h4>
                  <p className="text-sm text-slate-500">
                    {job.department || 'General'} · {job.location || 'Location TBD'} · {job.jobType}
                  </p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button
                    onClick={() => {
                      setActiveJob(job)
                      setShareOpen(true)
                    }}
                  >
                    Share job
                  </Button>
                  <Button
                    variant="dark"
                    onClick={() => {
                      setActiveJob(job)
                      setReferOpen(true)
                    }}
                  >
                    Refer friend
                  </Button>
                  {isHr ? (
                    <Button
                      onClick={() => {
                        setEditJobId(job.jobId)
                        setEditOpen(true)
                      }}
                    >
                      Edit job
                    </Button>
                  ) : null}
                  {isHr ? (
                    <Button
                      variant="dark"
                      onClick={() => {
                        setReferralJob(job)
                        setSelectedReferralId(null)
                        setReferralsOpen(true)
                      }}
                    >
                      Track referrals
                    </Button>
                  ) : null}
                </div>
              </div>
              <p className="mt-3 text-sm text-slate-600">{job.jobSummary || 'Summary unavailable.'}</p>
              <div className="mt-3 flex flex-wrap gap-3 text-xs text-slate-500">
                <span>Experience: {job.experienceRequired || 'Not specified'}</span>
                <span>HR owner: {job.hrOwnerEmail || 'Not assigned'}</span>
                <span>Reviewers: {job.cvReviewerEmails?.length ? job.cvReviewerEmails.join(', ') : 'Not assigned'}</span>
              </div>
            </div>
          ))}
        </div>
      </Card>

      <Modal
        title={activeJob ? `Share ${activeJob.jobTitle}` : 'Share job'}
        isOpen={shareOpen}
        onClose={() => setShareOpen(false)}
      >
        <div className="space-y-4">
          <Input
            label="Recipient emails (comma-separated)"
            value={shareRecipients}
            onChange={(event) => setShareRecipients(event.target.value)}
          />
          <Button onClick={handleShare} disabled={shareJob.isPending}>
            {shareJob.isPending ? 'Sharing...' : 'Send share email'}
          </Button>
        </div>
      </Modal>

      <Modal
        title={activeJob ? `Refer for ${activeJob.jobTitle}` : 'Refer friend'}
        isOpen={referOpen}
        onClose={() => setReferOpen(false)}
      >
        <div className="space-y-4">
          <Input
            label="Friend name"
            value={referralFriendName}
            onChange={(event) => setReferralFriendName(event.target.value)}
          />
          <Input
            label="Friend email (optional)"
            value={referralFriendEmail}
            onChange={(event) => setReferralFriendEmail(event.target.value)}
          />
          <label className="block space-y-2 text-sm">
            <span className="font-medium text-(--color-dark)">CV file</span>
            <input
              type="file"
              className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
              onChange={(event) => setReferralCv(event.target.files?.[0] ?? null)}
            />
          </label>
          <label className="block space-y-2 text-sm">
            <span className="font-medium text-(--color-dark)">Note (optional)</span>
            <textarea
              className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
              rows={3}
              value={referralNote}
              onChange={(event) => setReferralNote(event.target.value)}
            />
          </label>
          <Button onClick={handleRefer} disabled={createReferral.isPending}>
            {createReferral.isPending ? 'Sending...' : 'Submit referral'}
          </Button>
        </div>
      </Modal>

      <Modal
        title={editJobQuery.data ? `Edit ${editJobQuery.data.jobTitle}` : 'Edit job'}
        isOpen={editOpen}
        onClose={() => setEditOpen(false)}
      >
        <div className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <Input
              label="Job title"
              value={editForm.jobTitle}
              onChange={(event) => setEditForm((prev) => ({ ...prev, jobTitle: event.target.value }))}
            />
            <Input
              label="Department"
              value={editForm.department}
              onChange={(event) => setEditForm((prev) => ({ ...prev, department: event.target.value }))}
            />
            <Input
              label="Location"
              value={editForm.location}
              onChange={(event) => setEditForm((prev) => ({ ...prev, location: event.target.value }))}
            />
            <Select
              label="Job type"
              value={editForm.jobType}
              onChange={(event) =>
                setEditForm((prev) => ({
                  ...prev,
                  jobType: event.target.value as JobUpdatePayload['jobType']
                }))
              }
            >
              <option value="FullTime">Full time</option>
              <option value="PartTime">Part time</option>
              <option value="Internship">Internship</option>
            </Select>
            <Input
              label="Experience required"
              value={editForm.experienceRequired}
              onChange={(event) => setEditForm((prev) => ({ ...prev, experienceRequired: event.target.value }))}
            />
            <Input
              label="HR owner email"
              value={editForm.hrOwnerEmail}
              onChange={(event) => setEditForm((prev) => ({ ...prev, hrOwnerEmail: event.target.value }))}
            />
            <Input
              label="CV reviewer emails (comma-separated)"
              value={editReviewerEmails}
              onChange={(event) => setEditReviewerEmails(event.target.value)}
            />
            <Input
              label="JD file URL"
              value={editForm.jobDescriptionPath}
              onChange={(event) => setEditForm((prev) => ({ ...prev, jobDescriptionPath: event.target.value }))}
            />
          </div>
          <label className="block space-y-2 text-sm">
            <span className="font-medium text-(--color-dark)">Job summary</span>
            <textarea
              className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
              rows={3}
              value={editForm.jobSummary}
              onChange={(event) => setEditForm((prev) => ({ ...prev, jobSummary: event.target.value }))}
            />
          </label>
          <label className="flex items-center gap-2 text-sm font-medium">
            <input
              type="checkbox"
              checked={editForm.isActive}
              onChange={(event) => setEditForm((prev) => ({ ...prev, isActive: event.target.checked }))}
            />
            Active opening
          </label>
          <Button onClick={handleEditSubmit} disabled={updateJob.isPending}>
            {updateJob.isPending ? 'Saving...' : 'Update job'}
          </Button>
        </div>
      </Modal>

      <Modal
        title={referralJob ? `Referrals for ${referralJob.jobTitle}` : 'Referrals'}
        isOpen={referralsOpen}
        onClose={() => setReferralsOpen(false)}
      >
        <div className="space-y-4">
          {referralsQuery.isLoading ? <Spinner /> : null}
          {referrals.length === 0 && !referralsQuery.isLoading ? (
            <EmptyState title="No referrals" description="No referrals submitted yet for this job." />
          ) : null}
          {referrals.map((referral) => (
            <div key={referral.referralId} className="rounded-xl border border-slate-200 bg-white p-4">
              <div className="flex flex-col gap-2 md:flex-row md:items-start md:justify-between">
                <div>
                  <p className="text-sm font-semibold text-slate-900">{referral.friendName}</p>
                  <p className="text-xs text-slate-500">{referral.friendEmail || 'Email not provided'}</p>
                </div>
                <Button
                  onClick={() => setSelectedReferralId(referral.referralId)}
                >
                  View logs
                </Button>
              </div>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                <Select
                  label="Status"
                  value={statusById[referral.referralId] ?? referral.status}
                  onChange={(event) =>
                    setStatusById((prev) => ({
                      ...prev,
                      [referral.referralId]: event.target.value
                    }))
                  }
                >
                  {statusOptions.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </Select>
                <Input
                  label="Status note (optional)"
                  value={noteById[referral.referralId] ?? ''}
                  onChange={(event) =>
                    setNoteById((prev) => ({
                      ...prev,
                      [referral.referralId]: event.target.value
                    }))
                  }
                />
              </div>
              <div className="mt-3 flex items-center justify-between text-xs text-slate-500">
                <span>Submitted: {new Date(referral.submittedAt).toLocaleString()}</span>
                <Button
                  onClick={() => handleReferralStatusUpdate(referral)}
                  disabled={updateReferralStatus.isPending}
                >
                  {updateReferralStatus.isPending ? 'Updating...' : 'Update status'}
                </Button>
              </div>
            </div>
          ))}
          {selectedReferralId ? (
            <Card className="space-y-2">
              <h4 className="text-sm font-semibold">Status logs</h4>
              {referralLogsQuery.isLoading ? <Spinner /> : null}
              {referralLogs.length === 0 && !referralLogsQuery.isLoading ? (
                <p className="text-xs text-slate-500">No logs yet for this referral.</p>
              ) : null}
              {referralLogs.map((log: ReferralStatusLog) => (
                <div key={log.referralStatusLogId} className="rounded-md border border-slate-200 p-3 text-xs">
                  <p>
                    <span className="font-semibold">{log.oldStatus || 'New'}</span> →{' '}
                    <span className="font-semibold">{log.newStatus}</span>
                  </p>
                  <p className="text-slate-500">{new Date(log.changedAt).toLocaleString()}</p>
                  {log.note ? <p className="mt-1 text-slate-600">{log.note}</p> : null}
                </div>
              ))}
            </Card>
          ) : null}
        </div>
      </Modal>
    </div>
  )
}