import type { HTMLAttributes } from 'react'

type CardProps = HTMLAttributes<HTMLDivElement>

export const Card = ({ className, ...props }: CardProps) => (
  <div
    className={`rounded-2xl border border-slate-200 bg-white p-6 shadow-sm ${className}`}
    {...props}
  />
)