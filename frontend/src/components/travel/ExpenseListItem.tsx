import React, { memo } from 'react'
import { Badge } from '../ui/Badge'
import { Card } from '../ui/Card'
import { Review } from '../Review'
import { formatCurrency, formatDate } from '../../utils/format'
 
export interface Expense {
  expenseId: number
  amount: number
  currency: string
  expenseDate: string
  categoryId: number
  status: string
  remarks?: string
  proofs?: Array<{ proofId: number; fileName: string; filePath: string }>
  employeeId?: number
}
 
interface ExpenseListItemProps {
  expense: Expense
  isHr: boolean
  isEmployee: boolean
  onReview: (expenseId: number, formValues: { remarks: string; status: 'Approved' | 'Rejected' }) => void
  reviewPending: boolean
  children?: React.ReactNode
}
 
const statusTone = (status: string) => {
  if (status === 'Approved') return 'success'
  if (status === 'Rejected') return 'warning'
  if (status === 'Submitted') return 'info'
  return 'neutral'
}
 
export const ExpenseListItem = memo(
  ({ expense, isHr,  onReview, reviewPending, children }: ExpenseListItemProps) => (
    <div className="space-y-3">
      <Card className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-sm font-semibold text-slate-900">
            {formatCurrency(expense.amount, expense.currency)} · {formatDate(expense.expenseDate)}
          </p>
          <p className="text-xs text-slate-500">
            Expense ID: {expense.expenseId} · Category: {expense.categoryId}
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
          {isHr ? (
            <Review expenseId={expense.expenseId} onReview={onReview} isPending={reviewPending} />
          ) : null}
        </div>
      </Card>
      {children}
    </div>
  )
)
 
 