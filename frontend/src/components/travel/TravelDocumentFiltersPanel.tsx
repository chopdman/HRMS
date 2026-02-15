import { Controller, type Control } from 'react-hook-form'
import { AsyncSearchableSelect, SearchableSelect } from '../ui/Combobox'
import { Card } from '../ui/Card'
import { formatDate } from '../../utils/format'
import type { EmployeeOption } from '../../types/employee'
import type { TravelAssigned } from '../../types/travel'
import type { TravelDocumentFilters } from '../../hooks/travel/useTravelDocuments'

interface TravelDocumentFiltersPanelProps {
  control: Control<TravelDocumentFilters>
  employeeOptions: EmployeeOption[]
  onSearch: (query: string) => void
  isLoadingOptions: boolean
  travelOptions: TravelAssigned[] | undefined
  selectedTravelId?: number
  onEmployeeChange: (value: number | undefined) => void
  onTravelChange: (value: number | undefined) => void
}

export const TravelDocumentFiltersPanel = ({
  control,
  employeeOptions,
  onSearch,
  isLoadingOptions,
  travelOptions,
  selectedTravelId,
  onEmployeeChange,
  onTravelChange
}: TravelDocumentFiltersPanelProps) => (
  <Card className="space-y-4">
    <div>
      <h3 className="text-base font-semibold text-slate-900">Filter documents</h3>
      <p className="text-xs text-slate-500">Refine results using travel or employee identifiers.</p>
    </div>
    <div className="grid gap-4 md:grid-cols-2">
      <Controller
        name="employeeId"
        control={control}
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
              onEmployeeChange(value as number | undefined)
            }}
            onSearch={onSearch}
            isLoading={isLoadingOptions}
          />
        )}
      />
      <SearchableSelect
        label="Travel"
        options={
          travelOptions?.map((travel) => ({
            value: travel.travelId,
            label: `${travel.travelName} (${formatDate(travel.startDate)} → ${formatDate(travel.endDate)})`
          })) ?? []
        }
        value={selectedTravelId}
        onChange={(value) => onTravelChange(value as number | undefined)}
        error={undefined}
      />
    </div>
  </Card>
)