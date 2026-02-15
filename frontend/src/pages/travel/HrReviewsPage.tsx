import { useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { Review, type ReviewFormValues } from '../../components/Review'
import { Badge } from '../../components/ui/Badge'
import { Card } from '../../components/ui/Card'
import { EmptyState } from '../../components/ui/EmptyState'
import { Input } from '../../components/ui/Input'
import { Header } from '../../components/Header'
import { Spinner } from '../../components/ui/Spinner'
import { useHrExpenses, type ExpenseFilters } from '../../hooks/travel/useExpenses'
import { useReviewExpense } from '../../hooks/travel/useExpenseReview'
import { formatCurrency, formatDate } from '../../utils/format'
 
const statusTone = (status: string) => {
  if (status === 'Approved') return 'success'
  if (status === 'Rejected') return 'warning'
  if (status === 'Submitted') return 'info'
  return 'neutral'
}
 
export const HrReviewsPage = () => {
  const { register, watch } = useForm<ExpenseFilters>({
    defaultValues: {
      employeeId: undefined,
      travelId: undefined,
      status: 'Submitted',
      from: '',
      to: ''
    }
  })
 
  const filters = watch()
  const normalizedFilters = useMemo(
    () => ({
      employeeId: filters.employeeId ? Number(filters.employeeId) : undefined,
      travelId: filters.travelId ? Number(filters.travelId) : undefined,
      status: filters.status || undefined,
      from: filters.from || undefined,
      to: filters.to || undefined
    }),
    [filters]
  )
 
  const hrExpenses = useHrExpenses(normalizedFilters)
  const reviewExpense = useReviewExpense()
 
  const handleReview = async (expenseId: number, values: ReviewFormValues) => {
    await reviewExpense.mutateAsync({
      expenseId,
      status: values.status,
      remarks: values.remarks
    })
    await hrExpenses.refetch()
  }
 
  return (
    <section className="space-y-6">
      <Header
        title="HR expense reviews"
        description="Approve or reject submitted expenses with required remarks on rejection."
      />
 
      <Card className="space-y-4">
        <div>
          <h3 className="text-base font-semibold text-slate-900">Filters</h3>
          <p className="text-xs text-slate-500">Filter by employee, travel, status, or date range.</p>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
          <Input label="Employee ID" type="number" placeholder="e.g. 24" {...register('employeeId')} />
          <Input label="Travel ID" type="number" placeholder="e.g. 18" {...register('travelId')} />
          <Input label="Status" placeholder="Submitted / Approved / Rejected" {...register('status')} />
          <Input label="From" type="date" {...register('from')} />
          <Input label="To" type="date" {...register('to')} />
        </div>
      </Card>
 
      {hrExpenses.isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading expenses...
        </div>
      ) : null}
 
      {hrExpenses.isError ? (
        <Card>
          <p className="text-sm text-red-600">Unable to load expenses right now.</p>
        </Card>
      ) : null}
 
      {hrExpenses.data?.length ? (
        <div className="space-y-3">
          {hrExpenses.data.map((expense) => (
            <Card key={expense.expenseId} className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  {formatCurrency(expense.amount, expense.currency)} · {formatDate(expense.expenseDate)}
                </p>
                <p className="text-xs text-slate-500">
                  Expense ID: {expense.expenseId} · Category: {expense.categoryId} · Employee: {expense.employeeId}
                </p>
                {expense.proofs?.length ? (
                  <div className="text-xs text-slate-500">
                    <span className="font-medium">Proofs:</span>
                    <div className="mt-1 flex flex-wrap gap-2">
                      {expense.proofs.map((proof) => (
                        <a
                          key={proof.proofId}
                          className="text-xs font-semibold text-brand-600 hover:text-brand-700"
                          href={proof.filePath}
                          target="_blank"
                          rel="noreferrer"
                        >
                          {proof.fileName || `Proof ${proof.proofId}`}
                        </a>
                      ))}
                    </div>
                  </div>
                ) : (
                  <p className="text-xs text-slate-500">Proofs: none</p>
                )}
                {expense.remarks ? <p className="text-xs text-slate-500">Remarks: {expense.remarks}</p> : null}
              </div>
              <div className="flex flex-col items-start gap-3 md:items-end">
                <Badge tone={statusTone(expense.status)}>{expense.status}</Badge>
                <Review
                  expenseId={expense.expenseId}
                  onReview={handleReview}
                  isPending={reviewExpense.isPending}
                />
              </div>
            </Card>
          ))}
        </div>
      ) : null}
 
      {!hrExpenses.isLoading && !hrExpenses.isError && !hrExpenses.data?.length ? (
        <EmptyState title="No expenses to review" description="Submitted expenses will appear here." />
      ) : null}
    </section>
  )
}
 