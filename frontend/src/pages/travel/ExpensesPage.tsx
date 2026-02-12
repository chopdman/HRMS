import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Review, type ReviewFormValues } from '../../components/Review'
import { Badge } from '../../components/ui/Badge'
import { Card } from '../../components/ui/Card'
import { EmptyState } from '../../components/ui/EmptyState'
import { Input } from '../../components/ui/Input'
import { Header } from '../../components/Header'
import { SearchableSelect } from '../../components/ui/Combobox'
import { Select } from '../../components/ui/Select'
import { Spinner } from '../../components/ui/Spinner'
import { useAuth } from '../../hooks/useAuth'
import { useExpenseCategories } from '../../hooks/travel/useExpenseConfig'
import { useHrExpenses, useMyExpenses, type ExpenseFilters } from '../../hooks/travel/useExpenses'
import { useCreateExpenseDraft, useSubmitExpense, useUploadExpenseProof } from '../../hooks/travel/useExpenseMutations'
import { useReviewExpense } from '../../hooks/travel/useExpenseReview'
import { useTravelAssignments } from '../../hooks/travel/useTravel'
import { formatCurrency, formatDate } from '../../utils/format'

type ExpenseFormValues = {
  assignId?: number
  categoryId: number
  amount: number
  currency: string
  expenseDate: string
  proof: FileList
}

const statusTone = (status: string) => {
  if (status === 'Approved') return 'success'
  if (status === 'Rejected') return 'warning'
  if (status === 'Submitted') return 'info'
  return 'neutral'
}

export const ExpensesPage = () => {
  const { role } = useAuth()
  const isHr = role === 'HR'
  const isEmployee = role === 'Employee'
  const [successMessage, setSuccessMessage] = useState('')
  const { register, watch } = useForm<ExpenseFilters>({
    defaultValues: {
      employeeId: undefined,
      travelId: undefined,
      status: '',
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

  const myExpenses = useMyExpenses()
  const hrExpenses = useHrExpenses(normalizedFilters)

  const categoriesQuery = useExpenseCategories()
  const createDraft = useCreateExpenseDraft()
  const uploadProof = useUploadExpenseProof()
  const submitExpense = useSubmitExpense()
  const reviewExpense = useReviewExpense()
  const assignmentsQuery = useTravelAssignments(undefined, isEmployee)
  const assignmentsLoading = assignmentsQuery.isLoading

  const {
    register: registerForm,
    handleSubmit,
    watch: watchForm,
    setValue,
    reset,
    formState: { errors }
  } = useForm<ExpenseFormValues>({
    defaultValues: {
      assignId: undefined,
      categoryId: undefined,
      amount: undefined,
      currency: 'USD',
      expenseDate: ''
    }
  })

  const list = isHr ? hrExpenses.data : myExpenses.data
  const isLoading = isHr ? hrExpenses.isLoading : myExpenses.isLoading
  const isError = isHr ? hrExpenses.isError : myExpenses.isError

  const handleCreate = async (values: ExpenseFormValues) => {
    setSuccessMessage('')
    const proofFile = values.proof?.item(0)
    if (!proofFile) {
      return
    }

    const draft = await createDraft.mutateAsync({
      assignId: Number(values.assignId),
      categoryId: Number(values.categoryId),
      amount: Number(values.amount),
      currency: values.currency,
      expenseDate: values.expenseDate
    })
    console.log(draft.data.expenseId);
    await uploadProof.mutateAsync({ expenseId: draft?.data.expenseId, file: proofFile })
    await submitExpense.mutateAsync(draft.data.expenseId)

    reset()
    await myExpenses.refetch()
    setSuccessMessage('Expense submitted successfully.')
  }

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
        title="Expenses"
        description={isHr ? 'Review submitted expenses and manage approvals.' : 'Track submitted expenses and approvals.'}
      />

      {isEmployee ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Submit an expense</h3>
            <p className="text-xs text-slate-500">Add a proof document to submit your expense.</p>
          </div>
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit(handleCreate)}>
            <input
              type="hidden"
              {...registerForm('assignId', { required: 'Assignment is required.', valueAsNumber: true })}
            />
            <SearchableSelect
              label="Assignment"
              options={
                assignmentsQuery.data?.map((assignment) => ({
                  value: assignment.assignId,
                  label: `${assignment.travelName} (${formatDate(assignment.startDate)} → ${formatDate(assignment.endDate)})`
                })) ?? []
              }
              value={watchForm('assignId')}
              onChange={(value) => {
                setValue('assignId', value)
              }}
              error={errors.assignId?.message}
              disabled={assignmentsLoading}
            />
            {assignmentsLoading ? (
              <p className="text-xs text-slate-500 md:col-span-2">Loading assignments...</p>
            ) : null}
            {!assignmentsLoading && !assignmentsQuery.data?.length ? (
              <p className="text-xs text-amber-600 md:col-span-2">
                No assignments found. Ask HR to assign a travel plan.
              </p>
            ) : null}
            <Select
              label="Category"
              error={errors.categoryId?.message}
              {...registerForm('categoryId', { required: 'Category is required.', valueAsNumber: true })}
            >
              <option value="">Select category</option>
              {categoriesQuery.data?.map((category) => (
                <option key={category.categoryId} value={category.categoryId}>
                  {category.categoryName} (Max {category.maxAmountPerDay})
                </option>
              ))}
            </Select>
            <Input
              label="Amount"
              type="number"
              step="0.01"
              error={errors.amount?.message}
              {...registerForm('amount', {
                required: 'Amount is required.',
                valueAsNumber: true,
                min: { value: 0.01, message: 'Amount must be positive.' }
              })}
            />
            <Input
              label="Currency"
              error={errors.currency?.message}
              {...registerForm('currency', { required: 'Currency is required.' })}
            />
            <Input
              label="Expense date"
              type="date"
              error={errors.expenseDate?.message}
              {...registerForm('expenseDate', {
                required: 'Date is required.',
                validate: (value) => {
                  const assignment = assignmentsQuery.data?.find(
                    (item) => item.assignId === Number(watchForm('assignId'))
                  )

                  if (!assignment) {
                    return 'Select an assignment to validate the date.'
                  }

                  const start = new Date(assignment.startDate)
                  const end = new Date(assignment.endDate)
                  const allowedEnd = new Date(end)
                  allowedEnd.setDate(end.getDate() + 10)
                  const selected = new Date(value)

                  if (selected < start || selected > allowedEnd) {
                    return 'Expense date must be within travel dates and 10 days after end date.'
                  }

                  return true
                }
              })}
            />
            <Input
              label="Proof file"
              type="file"
              error={errors.proof?.message}
              {...registerForm('proof', { required: 'Proof file is required.' })}
            />
            <div className="md:col-span-2">
              <button
                className="inline-flex w-full items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-black hover:bg-brand-700 disabled:opacity-70"
                type="submit"
                disabled={
                  createDraft.isPending ||
                  uploadProof.isPending ||
                  submitExpense.isPending ||
                  assignmentsLoading
                }
              >
                {createDraft.isPending || uploadProof.isPending || submitExpense.isPending
                  ? 'Submitting...'
                  : 'Submit expense'}
              </button>
            </div>
          </form>
          {successMessage ? (
            <p className="text-sm text-emerald-600">{successMessage}</p>
          ) : null}
        </Card>
      ) : null}

      {isHr ? (
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
      ) : null}

      {isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading expenses...
        </div>
      ) : null}

      {isError ? (
        <Card>
          <p className="text-sm text-red-600">Unable to load expenses right now.</p>
        </Card>
      ) : null}

      {list?.length ? (
        <div className="space-y-3">
          {list.map((expense) => (
            <Card key={expense.expenseId} className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  {formatCurrency(expense.amount, expense.currency)} · {formatDate(expense.expenseDate)}
                </p>
                <p className="text-xs text-slate-500">
                  Expense ID: {expense.expenseId} · Category: {expense.categoryId}
                </p>
                {expense.remarks ? <p className="text-xs text-slate-500">Remarks: {expense.remarks}</p> : null}
              </div>
              <div className="flex flex-col items-start gap-3 md:items-end">
                <Badge tone={statusTone(expense.status)}>{expense.status}</Badge>
                {isHr ? (
                  <Review
                    expenseId={expense.expenseId}
                    onReview={handleReview}
                    isPending={reviewExpense.isPending}
                  />
                ) : null}
              </div>
            </Card>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && !list?.length ? (
        <EmptyState title="No expenses yet" description="Expenses will appear here once submitted." />
      ) : null}
    </section>
  )
}