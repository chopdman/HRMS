export type ExpenseCategory = {
  categoryId: number
  categoryName: string
  maxAmountPerDay: number
}

export type ExpenseRule = {
  ruleId: number
  ruleKey: string
  ruleValue: string
  scopeType: string
  categoryId?: number | null
  isActive: boolean
}