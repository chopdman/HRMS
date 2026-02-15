import { useEffect, useMemo, useState } from 'react'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
 
export type ComboOption = {
  value: number
  label: string
}
 
type SearchableSelectProps = {
  label: string
  options: ComboOption[]
  value?: number
  placeholder?: string
  error?: string
  disabled?: boolean
  onChange: (value?: number) => void
}
 
export const SearchableSelect = ({
  label,
  options,
  value,
  placeholder = 'Search…',
  error,
  disabled,
  onChange
}: SearchableSelectProps) => {
  const [query, setQuery] = useState('')
  const [selectedOverride, setSelectedOverride] = useState('')
 
  useEffect(() => {
    if (value === undefined) {
      setSelectedOverride('')
      return
    }
 
    const match = options.find((option) => option.value === value)
    if (match) {
      setSelectedOverride(match.label)
    }
  }, [options, value])
  const filtered = useMemo(() => {
    const lowered = query.toLowerCase()
    return options.filter((option) => option.label.toLowerCase().includes(lowered))
  }, [options, query])
 
  const selectedLabel = options.find((option) => option.value === value)?.label ?? selectedOverride
 
  return (
    <label className="block space-y-2 text-sm">
      <span className="font-medium text-slate-700 pr-2">{label}</span>
      <input
        className={`
          'w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-200'
          ${error && 'border-red-400 focus:border-red-400 focus:ring-red-200'}
          ${disabled && 'bg-slate-100 text-slate-400'}
        `}
        value={query}
        placeholder={selectedLabel || placeholder}
        disabled={disabled}
        onChange={(event) => setQuery(event.target.value)}
      />
      <div className="max-h-48 overflow-y-auto rounded-md border border-slate-200 bg-white">
        {filtered.length ? (
          filtered.map((option) => (
            <button
              key={option.value}
              type="button"
              className={`
                'flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-brand-50'
                ${value === option.value && 'bg-brand-50 text-brand-700'}
              `}
              onClick={() => {
                onChange(option.value)
                setSelectedOverride(option.label)
                setQuery('')
              }}
            >
              <span>{option.label}</span>
              {value === option.value ? <span className="text-xs">Selected</span> : null}
            </button>
          ))
        ) : (
          <div className="px-3 py-2 text-xs text-slate-500">No matches</div>
        )}
      </div>
      {value ? (
        <button
          type="button"
          className="text-xs font-semibold  text-slate-500 hover:text-slate-700"
          onClick={() => {
            onChange(undefined)
            setSelectedOverride('')
          }}
        >
          Clear selection
        </button>
      ) : null}
      {error ? <span className="text-xs text-red-500">{error}</span> : null}
    </label>
  )
}
 
type AsyncSelectProps = {
  label: string
  options: ComboOption[]
  value?: number
  placeholder?: string
  error?: string
  isLoading?: boolean
  disabled?: boolean
  onChange: (value?: number) => void
  onSearch: (query: string) => void
}
 
export const AsyncSearchableSelect = ({
  label,
  options,
  value,
  placeholder = 'Search…',
  error,
  isLoading,
  disabled,
  onChange,
  onSearch
}: AsyncSelectProps) => {
  const [query, setQuery] = useState('')
  const [selectedOverride, setSelectedOverride] = useState('')
  const debounced = useDebouncedValue(query, 300)
 
  useEffect(() => {
    onSearch(debounced.trim())
  }, [debounced, onSearch])
 
  useEffect(() => {
    if (value === undefined) {
      setSelectedOverride('')
      return
    }
 
    const match = options.find((option) => option.value === value)
    if (match) {
      setSelectedOverride(match.label)
    }
  }, [options, value])
 
  const selectedLabel = options.find((option) => option.value === value)?.label ?? selectedOverride
 
  return (
    <label className="block space-y-2 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        className={`
          'w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-200'
          ${error && 'border-red-400 focus:border-red-400 focus:ring-red-200'}
          ${disabled && 'bg-slate-100 text-slate-400'}
        `}
        value={query}
        placeholder={selectedLabel || placeholder}
        disabled={disabled}
        onChange={(event) => setQuery(event.target.value)}
      />
      <div className="max-h-48 overflow-y-auto rounded-md border border-slate-200 bg-white">
        {isLoading ? (
          <div className="px-3 py-2 text-xs text-slate-500">Loading...</div>
        ) : null}
        {!isLoading && options.length ? (
          options.map((option) => (
            <button
              key={option.value}
              type="button"
              className={`
                'flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-brand-50'
                ${value === option.value && 'bg-brand-50 text-brand-700'}
              `}
              onClick={() => {
                onChange(option.value)
                setSelectedOverride(option.label)
                setQuery('')
              }}
            >
              <span>{option.label}</span>
              {value === option.value ? <span className="text-xs">Selected</span> : null}
            </button>
          ))
        ) : null}
        {!isLoading && !options.length ? (
          <div className="px-3 py-2 text-xs text-slate-500">No matches</div>
        ) : null}
      </div>
      {value ? (
        <button
          type="button"
          className="text-xs font-semibold text-slate-500 hover:text-slate-700"
          onClick={() => {
            onChange(undefined)
            setSelectedOverride('')
          }}
        >
          Clear selection
        </button>
      ) : null}
      {error ? <span className="text-xs text-red-500">{error}</span> : null}
    </label>
  )
}
 
type AsyncMultiSelectProps = {
  label: string
  options: ComboOption[]
  value: number[]
  placeholder?: string
  error?: string
  isLoading?: boolean
  disabled?: boolean
  onChange: (value: number[]) => void
  onSearch: (query: string) => void
}
 
export const AsyncSearchableMultiSelect = ({
  label,
  options,
  value,
  placeholder = 'Search…',
  error,
  isLoading,
  disabled,
  onChange,
  onSearch
}: AsyncMultiSelectProps) => {
  const [query, setQuery] = useState('')
  const debounced = useDebouncedValue(query, 300)
 
  useEffect(() => {
    onSearch(debounced.trim())
  }, [debounced, onSearch])
 
  const toggleValue = (selected: number) => {
    if (value.includes(selected)) {
      onChange(value.filter((item) => item !== selected))
    } else {
      onChange([...value, selected])
    }
  }
 
  return (
    <label className="block space-y-2 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        className={`
          'w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-200',
          ${error && 'border-red-400 focus:border-red-400 focus:ring-red-200'}
          ${disabled && 'bg-slate-100 text-slate-400'}
        `}
        value={query}
        placeholder={placeholder}
        disabled={disabled}
        onChange={(event) => setQuery(event.target.value)}
      />
      <div className="max-h-56 overflow-y-auto rounded-md border border-slate-200 bg-white">
        {isLoading ? <div className="px-3 py-2 text-xs text-slate-500">Loading...</div> : null}
        {!isLoading && options.length ? (
          options.map((option) => (
            <label key={option.value} className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-brand-50">
              <input
                type="checkbox"
                checked={value.includes(option.value)}
                onChange={() => toggleValue(option.value)}
              />
              <span>{option.label}</span>
            </label>
          ))
        ) : null}
        {!isLoading && !options.length ? (
          <div className="px-3 py-2 text-xs text-slate-500">No matches</div>
        ) : null}
      </div>
      {value.length ? <div className="text-xs text-slate-500">Selected: {value.length}</div> : null}
      {error ? <span className="text-xs text-red-500">{error}</span> : null}
    </label>
  )
}
 
type SearchableMultiSelectProps = {
  label: string
  options: ComboOption[]
  value: number[]
  placeholder?: string
  error?: string
  disabled?: boolean
  onChange: (value: number[]) => void
}
 
export const SearchableMultiSelect = ({
  label,
  options,
  value,
  placeholder = 'Search…',
  error,
  disabled,
  onChange
}: SearchableMultiSelectProps) => {
  const [query, setQuery] = useState('')
  const filtered = useMemo(() => {
    const lowered = query.toLowerCase()
    return options.filter((option) => option.label.toLowerCase().includes(lowered))
  }, [options, query])
 
  const toggleValue = (selected: number) => {
    if (value.includes(selected)) {
      onChange(value.filter((item) => item !== selected))
    } else {
      onChange([...value, selected])
    }
  }
 
  return (
    <label className="block space-y-2 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        className={`
          'w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-200',
          ${error && 'border-red-400 focus:border-red-400 focus:ring-red-200'}
          ${disabled && 'bg-slate-100 text-slate-400'}
        `}
        value={query}
        placeholder={placeholder}
        disabled={disabled}
        onChange={(event) => setQuery(event.target.value)}
      />
      <div className="max-h-56 overflow-y-auto rounded-md border border-slate-200 bg-white">
        {filtered.length ? (
          filtered.map((option) => (
            <label key={option.value} className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-brand-50">
              <input
                type="checkbox"
                checked={value.includes(option.value)}
                onChange={() => toggleValue(option.value)}
              />
              <span>{option.label}</span>
            </label>
          ))
        ) : (
          <div className="px-3 py-2 text-xs text-slate-500">No matches</div>
        )}
      </div>
      {value.length ? (
        <div className="text-xs text-slate-500">Selected: {value.length}</div>
      ) : null}
      {error ? <span className="text-xs text-red-500">{error}</span> : null}
    </label>
  )
}
 