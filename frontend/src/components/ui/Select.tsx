import type { SelectHTMLAttributes } from "react";

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label: string;
  error?: string;
};

export const Select = ({
  label,
  error,
  className,
  children,
  ...props
}: SelectProps) => (
  <label className="block space-y-2 text-sm">
    <span className="font-medium text-slate-700 pr-3">{label}</span>
    <select
      className={` 'w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-100  focus:outline-none focus:ring-1 focus:ring-brand-200 '
        ${error && "border-red-400 focus:border-red-400 focus:ring-red-200"}
        ${className}
      `}
      {...props}
    >
      {children}
    </select>
    {error ? <span className="text-xs text-red-500">{error}</span> : null}
  </label>
);
