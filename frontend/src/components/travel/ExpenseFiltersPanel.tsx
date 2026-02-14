import { type UseFormRegister } from 'react-hook-form'
import { Card } from '../ui/Card'
import { Input } from '../ui/Input'
 
export interface ExpenseFilters {
  employeeId?: number | string
  travelId?: number | string
  status?: string
  from?: string
  to?: string
}
 
interface ExpenseFiltersProps {
  register: UseFormRegister<ExpenseFilters>
}
 
export const ExpenseFiltersPanel = ({ register }: ExpenseFiltersProps) => (
  <Card className="space-y-4">
    <div>
      <h3 className="text-base font-semibold text-slate-900">Filters</h3>
      <p className="text-xs text-slate-500">Use filters to narrow down expenses for review.</p>
    </div>
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
      <Input label="Employee ID" type="number" placeholder="e.g. 24" {...register('employeeId')} />
      <Input label="Travel ID" type="number" placeholder="e.g. 18" {...register('travelId')} />
      <Input label="Status" placeholder="Submitted / Approved / Rejected" {...register('status')} />
      <Input label="From" type="date" {...register('from')} />
      <Input label="To" type="date" {...register('to')} />
    </div>
  </Card>
)
 