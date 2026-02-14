import type { ButtonHTMLAttributes } from 'react'

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'dark'
}

export const Button = ({ className, variant = 'primary', ...props }: ButtonProps) => (
  <button
    className={`
      border bg-(--color-primary) inline-flex cursor-pointer items-center justify-center gap-1 rounded-md px-4 py-2 text-sm font-semibold transition focus:outline-none focus:ring-2 focus:ring-brand-400 focus:ring-offset-2
      ${variant === 'primary'
        ? 'bg-brand-600 text-white hover:bg-brand-700'
        : 'text-brand-600 hover:bg-brand-50'}
      ${className}
    `}
    {...props}
  />
)
