import type { InputHTMLAttributes } from 'react'

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string
  error?: string
}

export const Input = ({ label, error, className, ...props }: InputProps) => (
  <label className="block space-y-2 text-sm">
    <span className="font-medium text-slate-700">{label}</span>
    <input
      className={
        `w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-200
        ${error && `border-red-400 focus:border-red-400 focus:ring-red-200`}
        ${className}
      `}
      {...props}
    />
    {error ? <span className="text-xs text-red-500">{error}</span> : null}
  </label>
)