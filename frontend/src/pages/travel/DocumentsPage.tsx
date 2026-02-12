import { useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Card } from '../../components/ui/Card'
import { EmptyState } from '../../components/ui/EmptyState'
import { AsyncSearchableSelect, SearchableSelect } from '../../components/ui/Combobox'
import { Input } from '../../components/ui/Input'
import { Header } from '../../components/Header'
import { Spinner } from '../../components/ui/Spinner'
import { useAuth } from '../../hooks/useAuth'
import { useEmployeeSearch } from '../../hooks/useEmployeeSearch'
import { useTeamMembers } from '../../hooks/travel/useManager'
import { useUploadTravelDocument } from '../../hooks/travel/useTravelDocumentMutations'
import { useAssignedTravels } from '../../hooks/travel/useTravel'
import { useTravelDocuments, type TravelDocumentFilters } from '../../hooks/travel/useTravelDocuments'
import { formatDate } from '../../utils/format'

type DocumentFormValues = {
  assignId: number
  travelId: number
  employeeId?: number
  documentType: string
  file: FileList
}

export const DocumentsPage = () => {
  const { role } = useAuth()
  const canUpload = role === 'HR' || role === 'Employee'
  const isHr = role === 'HR'
  const isManager = role === 'Manager'
  const [message, setMessage] = useState('')
  const [searchQuery, setSearchQuery] = useState('')
  const employeesQuery = useEmployeeSearch(searchQuery, isHr && searchQuery.length >= 2)
  const teamMembersQuery = useTeamMembers()
  const {
    watch: watchFilters,
    control: controlFilters,
    setValue: setFilterValue
  } = useForm<TravelDocumentFilters>({
    defaultValues: {
      travelId: undefined,
      employeeId: undefined
    }
  })

  const {
    register: registerForm,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors }
  } = useForm<DocumentFormValues>({
    defaultValues: {
      assignId: undefined,
      travelId: undefined,
      employeeId: undefined,
      documentType: ''
    }
  })

  const filters = watchFilters()
  const selectedEmployeeId = filters.employeeId ? Number(filters.employeeId) : undefined
  const canFetchTravels = role === 'Employee' || Boolean(selectedEmployeeId)
  const travelOptionsQuery = useAssignedTravels(selectedEmployeeId, canFetchTravels)
  const normalizedFilters = useMemo(
    () => ({
      travelId: filters.travelId ? Number(filters.travelId) : undefined,
      employeeId: filters.employeeId ? Number(filters.employeeId) : undefined
    }),
    [filters]
  )

  const { data, isLoading, isError } = useTravelDocuments(normalizedFilters)
  const uploadMutation = useUploadTravelDocument()

  const employeeOptions = useMemo(() => {
    if (isHr) {
      return employeesQuery.data ?? []
    }

    if (isManager) {
      return (teamMembersQuery.data ?? []).map((member) => ({
        id: member.id,
        fullName: member.fullName,
        email: member.email
      }))
    }

    return []
  }, [employeesQuery.data, isHr, isManager, teamMembersQuery.data])

  const listLoading = isHr ? employeesQuery.isLoading : teamMembersQuery.isLoading

  const onUpload = async (values: DocumentFormValues) => {
    setMessage('')
    const file = values.file?.item(0)
    if (!file) {
      return
    }

    await uploadMutation.mutateAsync({
      assignId: Number(values.assignId),
      travelId: Number(values.travelId),
      employeeId: values.employeeId ? Number(values.employeeId) : undefined,
      documentType: values.documentType,
      file
    })

    reset()
    setMessage('Document uploaded successfully.')
  }

  return (
    <section className="space-y-6">
      <Header
        title="Travel documents"
        description="Browse uploaded travel documents and filter by travel or employee."
      />

      {canUpload ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Upload document</h3>
            <p className="text-xs text-slate-500">Provide assignment and travel details to upload.</p>
          </div>
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit(onUpload)}>
            <Input
              label="Assignment ID"
              type="number"
              error={errors.assignId?.message}
              {...registerForm('assignId', { required: 'Assignment ID is required.', valueAsNumber: true })}
            />
            <Input
              label="Travel ID"
              type="number"
              error={errors.travelId?.message}
              {...registerForm('travelId', { required: 'Travel ID is required.', valueAsNumber: true })}
            />
            <AsyncSearchableSelect
              label="Employee (optional)"
              error={errors.employeeId?.message}
              options={employeeOptions.map((employee) => ({
                value: employee.id,
                label: `${employee.fullName} (${employee.email})`
              }))}
              value={watch('employeeId')}
              onChange={(value) => {
                setValue('employeeId', value)
              }}
              onSearch={setSearchQuery}
              isLoading={listLoading}
            />
            <Input
              label="Document type"
              error={errors.documentType?.message}
              {...registerForm('documentType', { required: 'Document type is required.' })}
            />
            <Input
              label="File"
              type="file"
              error={errors.file?.message}
              {...registerForm('file', { required: 'File is required.' })}
            />
            <div className="md:col-span-2">
              <button
                className="inline-flex w-full items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-70"
                type="submit"
                disabled={uploadMutation.isPending}
              >
                {uploadMutation.isPending ? 'Uploading...' : 'Upload document'}
              </button>
            </div>
          </form>
          {message ? <p className="text-sm text-emerald-600">{message}</p> : null}
        </Card>
      ) : null}

      <Card className="space-y-4">
        <div>
          <h3 className="text-base font-semibold text-slate-900">Filter documents</h3>
          <p className="text-xs text-slate-500">Refine results using travel or employee identifiers.</p>
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <Controller
            name="employeeId"
            control={controlFilters}
            render={({ field }) => (
              <AsyncSearchableSelect
                label="Employee"
                options={employeeOptions.map((employee) => ({
                  value: employee.id,
                  label: `${employee.fullName} (${employee.email})`
                }))}
                value={field.value}
                onChange={(value) => {
                  field.onChange(value)
                  setFilterValue('travelId', undefined)
                }}
                onSearch={setSearchQuery}
                isLoading={listLoading}
              />
            )}
          />
          <SearchableSelect
            label="Travel"
            options={
              travelOptionsQuery.data?.map((travel) => ({
                value: travel.travelId,
                label: `${travel.travelName} (${formatDate(travel.startDate)} → ${formatDate(travel.endDate)})`
              })) ?? []
            }
            value={filters.travelId}
            onChange={(value) => setFilterValue('travelId', value)}
            error={undefined}
          />
        </div>
      </Card>

      {isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading documents...
        </div>
      ) : null}

      {isError ? (
        <Card>
          <p className="text-sm text-red-600">Unable to load documents right now.</p>
        </Card>
      ) : null}

      {data?.length ? (
        <div className="space-y-3">
          {data.map((doc) => (
            <Card key={doc.documentId} className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="text-sm font-semibold text-slate-900">{doc.documentType}</p>
                <p className="text-xs text-slate-500">{doc.fileName}</p>
                <p className="text-xs text-slate-500">Uploaded {formatDate(doc.uploadedAt)}</p>
              </div>
              <a
                className="text-sm font-semibold text-brand-600 hover:text-brand-700"
                href={doc.filePath}
                target="_blank"
                rel="noreferrer"
              >
                View file
              </a>
            </Card>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && !data?.length ? (
        <EmptyState
          title="No documents yet"
          description="Upload travel documents to see them listed here."
        />
      ) : null}
    </section>
  )
}