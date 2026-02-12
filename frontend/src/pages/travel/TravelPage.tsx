import { useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { AsyncSearchableMultiSelect, AsyncSearchableSelect } from '../../components/ui/Combobox'
import { Card } from '../../components/ui/Card'
import { EmptyState } from '../../components/ui/EmptyState'
import { Input } from '../../components/ui/Input'
import { Header } from '../../components/Header'
import { Spinner } from '../../components/ui/Spinner'
import { useAuth } from '../../hooks/useAuth'
import { useEmployeeSearch } from '../../hooks/useEmployeeSearch'
import { useTeamMembers } from '../../hooks/travel/useManager'
import { useAssignedTravels } from '../../hooks/travel/useTravel'
import { useCreateTravel } from '../../hooks/travel/useTravelMutations'
import { formatDate } from '../../utils/format'
import { Badge } from '../../components/ui/Badge'

type TravelCreateFormValues = {
  travelName: string
  destination: string
  purpose?: string
  startDate: string
  endDate: string
  assignments: number[]
}

type TravelFilterValues = {
  employeeId?: number
}



export const TravelsPage = () => {
  const { role, userId } = useAuth()
  const isEmployee = role === 'Employee'
  const isHr = role === 'HR'
  const isManager = role === 'Manager'
  const [message, setMessage] = useState('')
  const [searchQuery, setSearchQuery] = useState('')

  const { watch, control: controlFilter } = useForm<TravelFilterValues>({
    defaultValues: { employeeId: undefined }
  })

  const selectedEmployeeId = watch('employeeId')
  const normalizedEmployeeId = selectedEmployeeId ? Number(selectedEmployeeId) : undefined
  const canFetch = isEmployee || Boolean(normalizedEmployeeId)

  const { data, isLoading, isError } = useAssignedTravels(userId, canFetch)
  const createTravel = useCreateTravel()
  const employeesQuery = useEmployeeSearch(searchQuery, isHr && searchQuery.length >= 2)
  const teamMembersQuery =isManager ? useTeamMembers() : {isLoading:false,data:[],};

  const listLoading = isHr ? employeesQuery.isLoading : teamMembersQuery.isLoading

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors }
  } = useForm<TravelCreateFormValues>({
    defaultValues: {
      travelName: '',
      destination: '',
      purpose: '',
      startDate: '',
      endDate: '',
      assignments: []
    }
  })

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

  const onCreateTravel = async (values: TravelCreateFormValues) => {
    setMessage('')
    if (!userId) {
      setMessage('Unable to identify the current user.')
      return
    }

    if (!values.assignments?.length) {
      setMessage('Enter at least one employee ID to assign.')
      return
    }

    await createTravel.mutateAsync({
      travelName: values.travelName,
      destination: values.destination,
      purpose: values.purpose || undefined,
      startDate: values.startDate,
      endDate: values.endDate,
      createdById: userId,
      assignments: values.assignments.map((employeeId) => ({ employeeId }))
    })

    reset()
    setMessage('Travel created successfully.')
  }

  return (
    <section className="space-y-6">
      <Header
        title="Travels"
      />

      {isHr ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Create travel plan</h3>
            <p className="text-xs text-slate-500">Assign employees using the dropdown list.</p>
          </div>
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit(onCreateTravel)}>
            <Input
              label="Travel name"
              error={errors.travelName?.message}
              {...register('travelName', { required: 'Travel name is required.' })}
            />
            <Input
              label="Destination"
              error={errors.destination?.message}
              {...register('destination', { required: 'Destination is required.' })}
            />
            <Input label="Purpose" {...register('purpose')} />
            <Controller
              name="assignments"
              control={control}
              rules={{
                validate: (value) =>
                  value && value.length > 0 ? true : 'At least one employee must be assigned.'
              }}
              render={({ field }) => (
                <AsyncSearchableMultiSelect
                  label="Assignments"
                  options={ Array.isArray(employeeOptions) ? employeeOptions.map((employee) => ({
                    value: employee.id,
                    label: `${employee.fullName} (${employee.email})`
                  })): []}
                  value={field.value ?? []}
                  onChange={field.onChange}
                  onSearch={setSearchQuery}
                  isLoading={listLoading}
                  error={errors.assignments?.message}
                />
              )}
            />
            <Input
              label="Start date"
              type="date"
              error={errors.startDate?.message}
              {...register('startDate', { required: 'Start date is required.' })}
            />
            <Input
              label="End date"
              type="date"
              error={errors.endDate?.message}
              {...register('endDate', { required: 'End date is required.' })}
            />
            <div className="md:col-span-2">
              <button
                className="inline-flex w-full items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-black hover:bg-brand-700 disabled:opacity-70"
                type="submit"
                disabled={createTravel.isPending}
              >
                {createTravel.isPending ? 'Creating...' : 'Create travel'}
              </button>
            </div>
          </form>
          {message ? <p className="text-sm text-emerald-600">{message}</p> : null}
        </Card>
      ) : null}

      {!isEmployee ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Lookup assigned travels</h3>
            <p className="text-xs text-slate-500">Managers and HR must select an employee.</p>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <Controller
              name="employeeId"
              control={controlFilter}
              render={({ field }) => (
                // <AsyncSearchableSelect
                //   label="Employee"
                //   options={employeeOptions.map((employee) => ({
                //     value: employee.id,
                //     label: `${employee.fullName} (${employee.email})`
                //   }))}
                //   value={field.value}
                //   onChange={field.onChange}
                //   onSearch={setSearchQuery}
                //   isLoading={listLoading}
                // />
                <h1></h1>
              )}
            />
          </div>
        </Card>
      ) : null}

      {isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading assigned travels...
        </div>
      ) : null}

      {isError ? (
        <Card>
          <p className="text-sm text-red-600">Unable to load travels right now.</p>
        </Card>
      ) : null}

      {data?.length ? (
        <div className="grid gap-4 md:grid-cols-2">
          {data.map((travel) => (
            <Card key={travel.travelId} className="space-y-3">
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-slate-900">{travel.travelName}</h3>
                  <p className="text-sm text-slate-500">{travel.destination}</p>
                </div>
                <Badge tone="info">Assigned</Badge>
              </div>
              <div className="text-sm text-slate-600">
                <span className="font-medium">Dates:</span> {formatDate(travel.startDate)} → {formatDate(travel.endDate)}
              </div>
            </Card>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && !data?.length ? (
        <EmptyState
          title="No travel plans yet"
          description={
            canFetch
              ? 'Assigned travels will appear here once HR creates and assigns a plan.'
              : 'Enter an employee ID to view assigned travel plans.'
          }
        />
      ) : null}
    </section>
  )
}