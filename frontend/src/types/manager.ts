export type TeamMember = {
  id: number
  fullName: string
  email: string
  department?: string | null
  designation?: string | null
}

export type TeamExpense = {
  expenseId: number
  employeeId: number
  travelId: number
  categoryId: number
  amount: number
  currency: string
  expenseDate: string
  status: string
}