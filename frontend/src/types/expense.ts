export type ExpenseItem = {
  expenseId: number
  assignId: number
  categoryId: number
  amount: number
  currency: string
  expenseDate: string
  status: string
  submittedAt?: string | null
  reviewedById?: number | null
  reviewedAt?: string | null
  remarks?: string | null
}