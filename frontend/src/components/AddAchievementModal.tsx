import { useRef, useState } from 'react'
import { Modal } from './ui/Modal'
import { Input } from './ui/Input'
import { Button } from './ui/Button'
import { useCreateAchievementPost } from '../hooks/achievements/useAchievements'

type AddAchievementModalProps = {
  isOpen: boolean
  onClose: () => void
}

const parseTags = (raw: string) =>
  raw
    .split(',')
    .map((tag) => tag.trim())
    .filter(Boolean)

export const AddAchievementModal = ({ isOpen, onClose }: AddAchievementModalProps) => {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [tags, setTags] = useState('')
  const [attachment, setAttachment] = useState<File | null>(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const createPost = useCreateAchievementPost()

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) {
      setAttachment(file)
      setError('')
    }
  }

  const handleRemoveFile = () => {
    setAttachment(null)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  const handleSubmit = async () => {
    setMessage('')
    setError('')

    if (!title.trim()) {
      setError('Title is required.')
      return
    }

    try {
      const formData = new FormData()
      formData.append('title', title.trim())
      if (description.trim()) {
        formData.append('description', description.trim())
      }
      if (tags.trim()) {
        formData.append('tags', tags)
      }
      if (attachment) {
        formData.append('attachment', attachment)
      }

      await createPost.mutateAsync(formData as any)

      setTitle('')
      setDescription('')
      setTags('')
      setAttachment(null)
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      setMessage('Achievement shared with the team!')
      setTimeout(() => {
        onClose()
        setMessage('')
      }, 1500)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create achievement')
    }
  }

  const handleClose = () => {
    setTitle('')
    setDescription('')
    setTags('')
    setAttachment(null)
    setMessage('')
    setError('')
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
    onClose()
  }

  return (
    <Modal title="New Achievement" isOpen={isOpen} onClose={handleClose}>
      <div className="space-y-4">
        <Input label="Title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Share your achievement..." />
        <Input
          label="Tags"
          value={tags}
          onChange={(e) => setTags(e.target.value)}
          placeholder="culture, growth, kudos"
        />
        <label className="block space-y-2 text-sm">
          <span className="font-medium text-(--color-dark)">Description</span>
          <textarea
            className="min-h-[100px] w-full rounded-md border border-slate-200 bg-(--color-text) px-3 py-2 text-sm text-(--color-dark) shadow-sm focus:border-(--color-primary) focus:outline-none focus:ring-2 focus:ring-(--color-primary)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Tell us more about your achievement..."
          />
        </label>

        <div className="space-y-2">
          <label className="block text-sm font-medium text-(--color-dark)">Attachment (optional)</label>
          <div className="flex items-center gap-2">
            <input
              ref={fileInputRef}
              type="file"
              onChange={handleFileChange}
              className="hidden"
              accept="image/*,application/pdf,.doc,.docx,.xls,.xlsx"
            />
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
            >
              Choose File
            </button>
            {attachment && (
              <div className="flex items-center gap-2">
                <span className="text-sm text-slate-600">{attachment.name}</span>
                <button
                  type="button"
                  onClick={handleRemoveFile}
                  className="text-xs text-red-600 hover:text-red-700"
                >
                  Remove
                </button>
              </div>
            )}
          </div>
        </div>

        {error && <div className="rounded-md bg-red-50 p-3 text-sm text-red-700">{error}</div>}
        {message && <div className="rounded-md bg-emerald-50 p-3 text-sm text-emerald-700">{message}</div>}

        <div className="flex gap-2 pt-4">
          <Button
            type="button"
            disabled={createPost.isPending}
            onClick={handleSubmit}
          >
            {createPost.isPending ? 'Sharing...' : 'Share Achievement'}
          </Button>
          <Button type="button" variant="dark" onClick={handleClose}>
            Cancel
          </Button>
        </div>
      </div>
    </Modal>
  )
}
