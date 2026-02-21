import type { ReactNode } from 'react'

type ModalProps = {
  title: string
  isOpen: boolean
  onClose: () => void
  children: ReactNode
}

export const Modal = ({ title, isOpen, onClose, children }: ModalProps) => {
  if (!isOpen) {
    return null
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-lg rounded-2xl border border-slate-200 bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
          <button
            type="button"
            className="rounded-md border border-slate-200 px-2 py-1 text-sm font-semibold text-slate-600 hover:bg-slate-100"
            onClick={onClose}
          >
            Close
          </button>
        </div>
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  )
}