import { useMemo, useState } from 'react'
import { FiHeart, FiMessageCircle, FiEdit2, FiTrash2, FiChevronDown } from 'react-icons/fi'
import { Header } from '../../components/Header'
import { Card } from '../../components/ui/Card'
import { Input } from '../../components/ui/Input'
import { Button } from '../../components/ui/Button'
import { Spinner } from '../../components/ui/Spinner'
import { useAuth } from '../../hooks/useAuth'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { AddAchievementModal } from '../../components/AddAchievementModal'
import {
  useAchievementsFeed,
  useAddAchievementComment,
  useDeleteAchievementComment,
  useDeleteAchievementPost,
  useLikeAchievementPost,
  useUnlikeAchievementPost,
  useUpdateAchievementComment,
  useUpdateAchievementPost
} from '../../hooks/achievements/useAchievements'
import type { AchievementPost, AchievementComment, AchievementUser } from '../../types/achievements'

const parseTags = (raw: string) =>
  raw
    .split(',')
    .map((tag) => tag.trim())
    .filter(Boolean)

export const AchievementsPage = () => {
  const { userId, role } = useAuth()
  const isHr = role === 'HR'
  const [authorFilter, setAuthorFilter] = useState('')
  const [tagFilter, setTagFilter] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const debouncedAuthor = useDebouncedValue(authorFilter, 400)
  const debouncedTag = useDebouncedValue(tagFilter, 400)

  const feedQuery = useAchievementsFeed({
    author: debouncedAuthor || undefined,
    tag: debouncedTag || undefined,
    fromDate: fromDate || undefined,
    toDate: toDate || undefined
  })

  const updatePost = useUpdateAchievementPost()
  const deletePost = useDeleteAchievementPost()
  const addComment = useAddAchievementComment()
  const updateComment = useUpdateAchievementComment()
  const deleteComment = useDeleteAchievementComment()
  const likePost = useLikeAchievementPost()
  const unlikePost = useUnlikeAchievementPost()

  const [editingPostId, setEditingPostId] = useState<number | null>(null)
  const [editTitle, setEditTitle] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editTags, setEditTags] = useState('')

  const [commentDrafts, setCommentDrafts] = useState<Record<number, string>>({})
  const [editingCommentId, setEditingCommentId] = useState<number | null>(null)
  const [editCommentText, setEditCommentText] = useState('')
  const [openCommentBoxId, setOpenCommentBoxId] = useState<number | null>(null)
  const [hoveredLikePostId, setHoveredLikePostId] = useState<number | null>(null)
  const [replyingToCommentId, setReplyingToCommentId] = useState<number | null>(null)
  const [expandedReplies, setExpandedReplies] = useState<Set<number>>(new Set())

  const posts = useMemo(() => feedQuery.data ?? [], [feedQuery.data])

  const startEdit = (post: AchievementPost) => {
    setEditingPostId(post.postId)
    setEditTitle(post.title)
    setEditDescription(post.description ?? '')
    setEditTags(post.tags.join(', '))
  }

  const onUpdatePost = async (postId: number) => {
    await updatePost.mutateAsync({
      postId,
      title: editTitle.trim(),
      description: editDescription.trim() || undefined,
      tags: parseTags(editTags)
    })

    setEditingPostId(null)
    setEditTitle('')
    setEditDescription('')
    setEditTags('')
  }

  const onDeletePost = async (postId: number) => {
    const shouldDelete = window.confirm('Delete this post?')
    if (!shouldDelete) {
      return
    }

    const reason = isHr ? window.prompt('Reason for removal (optional):') ?? undefined : undefined
    await deletePost.mutateAsync({ postId, reason })
  }

  const onToggleLike = async (post: AchievementPost) => {
    if (post.hasLiked) {
      await unlikePost.mutateAsync(post.postId)
      return
    }

    await likePost.mutateAsync(post.postId)
  }

  const onAddComment = async (postId: number) => {
    const text = (commentDrafts[postId] ?? '').trim()
    if (!text) {
      return
    }

    await addComment.mutateAsync({ postId, commentText: text })
    setCommentDrafts((prev) => ({ ...prev, [postId]: '' }))
  }

  const onStartEditComment = (commentId: number, text: string) => {
    setEditingCommentId(commentId)
    setEditCommentText(text)
  }

  const onUpdateComment = async (commentId: number) => {
    await updateComment.mutateAsync({ commentId, commentText: editCommentText.trim() })
    setEditingCommentId(null)
    setEditCommentText('')
  }

  const onDeleteComment = async (commentId: number) => {
    const shouldDelete = window.confirm('Delete this comment?')
    if (!shouldDelete) {
      return
    }

    const reason = isHr ? window.prompt('Reason for removal (optional):') ?? undefined : undefined
    await deleteComment.mutateAsync({ commentId, reason })
  }

  return (
    <section className="space-y-6">
      <div className="flex items-center justify-between">
        <Header
          title="Achievements"
          description="Celebrate wins, milestones, and moments with the entire team."
        />
        <Button type="button" onClick={() => setIsModalOpen(true)}>
          New Achievement
        </Button>
      </div>

      <AddAchievementModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />

      <Card className="space-y-4">
        <div>
          <h3 className="text-base font-semibold text-slate-900">Filter the feed</h3>
          <p className="text-sm text-slate-500">Search by author, tag, or a date range.</p>
        </div>
        <div className="grid gap-4 md:grid-cols-4">
          <Input
            label="Author"
            value={authorFilter}
            onChange={(event) => setAuthorFilter(event.target.value)}
            placeholder="Search by name"
          />
          <Input
            label="Tag"
            value={tagFilter}
            onChange={(event) => setTagFilter(event.target.value)}
            placeholder="kudos"
          />
          <Input label="From" type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} />
          <Input label="To" type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} />
        </div>
      </Card>

      {feedQuery.isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading achievements...
        </div>
      ) : null}

      {feedQuery.isError ? (
        <Card>
          <p className="text-sm text-red-600">Unable to load achievements right now.</p>
        </Card>
      ) : null}

      {posts.length === 0 && !feedQuery.isLoading ? (
        <Card>
          <p className="text-sm text-slate-600">No achievements to display. Be the first to share!</p>
        </Card>
      ) : null}

      <div className="space-y-4">
        {posts.map((post: AchievementPost) => {
          const canEdit = !post.isSystemGenerated && post.author.id === userId
          const canDelete = (post.author.id === userId && !post.isSystemGenerated) || isHr
          const isEditing = editingPostId === post.postId

          return (
            <Card key={post.postId} className="space-y-4">
              <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="text-lg font-semibold text-slate-900">{post.title}</h3>
                    {post.isSystemGenerated ? (
                      <span className="rounded-full bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
                        System post
                      </span>
                    ) : null}
                  </div>
                  <p className="text-sm text-slate-500">
                    {post.author.fullName} · {new Date(post.createdAt).toLocaleString()}
                  </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {canEdit ? (
                    <Button type="button" variant="dark" onClick={() => startEdit(post)}>
                      Edit
                    </Button>
                  ) : null}
                  {canDelete ? (
                    <Button type="button" variant="dark" onClick={() => onDeletePost(post.postId)}>
                      Delete
                    </Button>
                  ) : null}
                </div>
              </div>

              {isEditing ? (
                <div className="grid gap-3 md:grid-cols-2">
                  <Input label="Title" value={editTitle} onChange={(event) => setEditTitle(event.target.value)} />
                  <Input label="Tags" value={editTags} onChange={(event) => setEditTags(event.target.value)} />
                  <label className="md:col-span-2 block space-y-2 text-sm">
                    <span className="font-medium text-(--color-dark)">Description</span>
                    <textarea
                      className="min-h-[110px] w-full rounded-md border border-slate-200 bg-(--color-text) px-3 py-2 text-sm text-(--color-dark) shadow-sm focus:border-(--color-primary) focus:outline-none focus:ring-2 focus:ring-(--color-primary)"
                      value={editDescription}
                      onChange={(event) => setEditDescription(event.target.value)}
                    />
                  </label>
                  <div className="md:col-span-2 flex flex-wrap items-center gap-2">
                    <Button type="button" onClick={() => onUpdatePost(post.postId)}>
                      Save
                    </Button>
                    <Button type="button" variant="dark" onClick={() => setEditingPostId(null)}>
                      Cancel
                    </Button>
                  </div>
                </div>
              ) : (
                <>
                  {post.description ? <p className="text-sm text-slate-700">{post.description}</p> : null}
                  {post.attachmentUrl && (
                    <div className="mt-3">
                      <a
                        href={post.attachmentUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center gap-2 rounded-md bg-slate-50 px-3 py-2 text-sm text-emerald-700 hover:bg-slate-100"
                      >
                        📎 {post.attachmentFileName || 'View Attachment'}
                      </a>
                    </div>
                  )}
                  {post.tags.length ? (
                    <div className="flex flex-wrap gap-2">
                      {post.tags.map((tag: string) => (
                        <span key={tag} className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                          #{tag}
                        </span>
                      ))}
                    </div>
                  ) : null}
                </>
              )}

              <div className="flex flex-wrap items-center gap-6 border-t border-slate-200 pt-4">
                <div
                  className="relative"
                  onMouseEnter={() => setHoveredLikePostId(post.postId)}
                  onMouseLeave={() => setHoveredLikePostId(null)}
                >
                  <button
                    type="button"
                    className={`flex items-center gap-2 transition-colors ${
                      post.hasLiked ? 'text-red-600' : 'text-slate-500 hover:text-red-600'
                    }`}
                    onClick={() => onToggleLike(post)}
                  >
                    <FiHeart className={`text-lg ${post.hasLiked ? 'fill-current' : ''}`} />
                    <span className="text-sm font-semibold">{post.likeCount}</span>
                  </button>
                  {hoveredLikePostId === post.postId && post.recentLikers.length > 0 && (
                    <div className="absolute bottom-full left-0 mb-2 z-10 bg-slate-900 text-white text-xs rounded-lg px-3 py-2 whitespace-nowrap shadow-lg">
                      <div className="font-semibold mb-1">Liked by:</div>
                      <div className="space-y-1">
                        {post.recentLikers.map((liker: AchievementUser) => (
                          <div key={liker.id}>{liker.fullName}</div>
                        ))}
                      </div>
                      <div className="absolute top-full left-1/2 -translate-x-1/2 w-0 h-0 border-l-4 border-r-4 border-t-4 border-l-transparent border-r-transparent border-t-slate-900"></div>
                    </div>
                  )}
                </div>
                <button
                  type="button"
                  className="flex items-center gap-2 text-slate-500 hover:text-emerald-600 transition-colors"
                  onClick={() => setOpenCommentBoxId(openCommentBoxId === post.postId ? null : post.postId)}
                >
                  <FiMessageCircle className="text-lg" />
                  <span className="text-sm font-semibold">{post.commentCount}</span>
                </button>
              </div>

              {(post.comments.length > 0 || openCommentBoxId === post.postId) && (
                <div className="space-y-4 border-t border-slate-200 pt-4">
                  {post.comments.length > 0 && (
                    <div className="space-y-3">
                      {post.comments
                        .filter((c: AchievementComment) => !c.parentCommentId)
                        .map((comment: AchievementComment) => (
                          <div key={comment.commentId}>
                            <CommentThread
                              comment={comment}
                              post={post}
                              userId={userId}
                              isHr={isHr}
                              editingCommentId={editingCommentId}
                              editCommentText={editCommentText}
                              commentDrafts={commentDrafts}
                              replyingToCommentId={replyingToCommentId}
                              expandedReplies={expandedReplies}
                              onStartEditComment={onStartEditComment}
                              onUpdateComment={onUpdateComment}
                              onDeleteComment={onDeleteComment}
                              onAddComment={addComment}
                              setEditingCommentId={setEditingCommentId}
                              setEditCommentText={setEditCommentText}
                              setCommentDrafts={setCommentDrafts}
                              setReplyingToCommentId={setReplyingToCommentId}
                              setExpandedReplies={setExpandedReplies}
                            />
                          </div>
                        ))}
                    </div>
                  )}

                  {openCommentBoxId === post.postId && (
                    <div className="flex gap-3 pt-4 border-t border-slate-200">
                      <div className="h-8 w-8 flex-shrink-0 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-xs font-semibold">
                        {/* Current user initial */}
                        {userId?.toString().charAt(0) || 'U'}
                      </div>
                      <div className="flex-1">
                        <textarea
                          className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                          placeholder="Add a comment..."
                          rows={2}
                          value={commentDrafts[post.postId] ?? ''}
                          onChange={(event) =>
                            setCommentDrafts((prev) => ({ ...prev, [post.postId]: event.target.value }))
                          }
                        />
                        <div className="mt-2 flex gap-2">
                          <button
                            type="button"
                            className="text-xs font-semibold text-emerald-700 hover:text-emerald-800"
                            onClick={() => onAddComment(post.postId)}
                          >
                            Comment
                          </button>
                          <button
                            type="button"
                            className="text-xs font-semibold text-slate-500 hover:text-slate-700"
                            onClick={() => setOpenCommentBoxId(null)}
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </Card>
          )
        })}
      </div>
    </section>
  )
}

interface CommentThreadProps {
  comment: AchievementComment
  post: AchievementPost
  userId?: number
  isHr: boolean
  editingCommentId: number | null
  editCommentText: string
  commentDrafts: Record<number, string>
  replyingToCommentId: number | null
  expandedReplies: Set<number>
  onStartEditComment: (commentId: number, text: string) => void
  onUpdateComment: (commentId: number) => Promise<void>
  onDeleteComment: (commentId: number) => Promise<void>
  onAddComment: ReturnType<typeof useAddAchievementComment>
  setEditingCommentId: (id: number | null) => void
  setEditCommentText: (text: string) => void
  setCommentDrafts: (fn: (prev: Record<number, string>) => Record<number, string>) => void
  setReplyingToCommentId: (id: number | null) => void
  setExpandedReplies: (fn: (prev: Set<number>) => Set<number>) => void
}

const CommentThread = ({
  comment,
  post,
  userId,
  isHr,
  editingCommentId,
  editCommentText,
  commentDrafts,
  replyingToCommentId,
  expandedReplies,
  onStartEditComment,
  onUpdateComment,
  onDeleteComment,
  onAddComment,
  setEditingCommentId,
  setEditCommentText,
  setCommentDrafts,
  setReplyingToCommentId,
  setExpandedReplies
}: CommentThreadProps) => {
  const isCommentOwner = comment.author.id === userId
  const canManageComment = isCommentOwner || isHr
  const isEditingComment = editingCommentId === comment.commentId
  const isReplyingToComment = replyingToCommentId === comment.commentId
  const replies = comment.replies || []
  const hasReplies = replies.length > 0
  const isRepliesExpanded = expandedReplies.has(comment.commentId)

  return (
    <div className="space-y-2">
      {/* Main Comment */}
      <div className="group flex gap-3 hover:bg-slate-50 px-2 py-2 rounded-lg transition-colors">
        {/* Avatar */}
        <div className="h-8 w-8 flex-shrink-0 rounded-full bg-gradient-to-br from-emerald-400 to-emerald-600 flex items-center justify-center text-white text-xs font-semibold">
          {comment.author.fullName.charAt(0)}
        </div>

        {/* Comment Content */}
        <div className="flex-1 min-w-0">
          {isEditingComment ? (
            <div className="space-y-2">
              <textarea
                className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                value={editCommentText}
                onChange={(event) => setEditCommentText(event.target.value)}
              />
              <div className="flex gap-2">
                <button
                  type="button"
                  className="text-xs font-semibold text-emerald-700 hover:text-emerald-800"
                  onClick={() => onUpdateComment(comment.commentId)}
                >
                  Save
                </button>
                <button
                  type="button"
                  className="text-xs font-semibold text-slate-500 hover:text-slate-700"
                  onClick={() => setEditingCommentId(null)}
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div>
              <div className="flex items-center gap-2">
                <p className="text-sm font-semibold text-slate-900">{comment.author.fullName}</p>
                <p className="text-xs text-slate-500">{new Date(comment.createdAt).toLocaleDateString()}</p>
              </div>
              <p className="mt-1 text-sm text-slate-800 break-words">{comment.commentText}</p>

              {/* Actions */}
              <div className="mt-2 flex items-center gap-4 text-xs font-semibold text-slate-500 opacity-0 group-hover:opacity-100 transition-opacity">
                <button
                  type="button"
                  className="flex items-center gap-1 hover:text-emerald-700"
                  onClick={() =>
                    setReplyingToCommentId(isReplyingToComment ? null : comment.commentId)
                  }
                >
                  <FiMessageCircle className="text-sm" />
                  Reply
                </button>
                {isCommentOwner && (
                  <button
                    type="button"
                    className="flex items-center gap-1 hover:text-blue-700"
                    onClick={() => onStartEditComment(comment.commentId, comment.commentText)}
                  >
                    <FiEdit2 className="text-sm" />
                    Edit
                  </button>
                )}
                {canManageComment && (
                  <button
                    type="button"
                    className="flex items-center gap-1 hover:text-red-700"
                    onClick={() => onDeleteComment(comment.commentId)}
                  >
                    <FiTrash2 className="text-sm" />
                    Delete
                  </button>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Reply Input Box */}
      {isReplyingToComment && (
        <div className="ml-11 flex gap-3">
          <div className="h-8 w-8 flex-shrink-0 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-xs font-semibold">
            {userId?.toString().charAt(0) || 'U'}
          </div>
          <div className="flex-1">
            <textarea
              className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500"
              placeholder="Write a reply..."
              rows={2}
              value={commentDrafts[comment.commentId] ?? ''}
              onChange={(event) =>
                setCommentDrafts((prev) => ({ ...prev, [comment.commentId]: event.target.value }))
              }
            />
            <div className="mt-2 flex gap-2">
              <button
                type="button"
                className="text-xs font-semibold text-emerald-700 hover:text-emerald-800"
                onClick={() => {
                  const text = (commentDrafts[comment.commentId] ?? '').trim()
                  if (!text) return
                  onAddComment.mutateAsync({
                    postId: post.postId,
                    commentText: text,
                    parentCommentId: comment.commentId
                  })
                  setCommentDrafts((prev) => ({ ...prev, [comment.commentId]: '' }))
                  setReplyingToCommentId(null)
                }}
              >
                Reply
              </button>
              <button
                type="button"
                className="text-xs font-semibold text-slate-500 hover:text-slate-700"
                onClick={() => setReplyingToCommentId(null)}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* View/Hide Replies Button */}
      {hasReplies && (
        <button
          type="button"
          className="ml-11 flex items-center gap-1 text-xs font-semibold text-slate-600 hover:text-emerald-700 transition-colors group/button"
          onClick={() => {
            setExpandedReplies((prev) => {
              const newExpanded = new Set(prev)
              if (newExpanded.has(comment.commentId)) {
                newExpanded.delete(comment.commentId)
              } else {
                newExpanded.add(comment.commentId)
              }
              return newExpanded
            })
          }}
        >
          <FiChevronDown
            className={`text-sm transition-transform ${
              isRepliesExpanded ? 'rotate-180' : ''
            }`}
          />
          {isRepliesExpanded
            ? 'Hide replies'
            : `View ${replies.length} ${replies.length === 1 ? 'reply' : 'replies'}`}
        </button>
      )}

      {/* Nested Replies */}
      {hasReplies && isRepliesExpanded && (
        <div className="ml-8 border-l-2 border-slate-200 pl-4 space-y-0">
          {replies.map((reply) => (
            <CommentThread
              key={reply.commentId}
              comment={reply}
              post={post}
              userId={userId}
              isHr={isHr}
              editingCommentId={editingCommentId}
              editCommentText={editCommentText}
              commentDrafts={commentDrafts}
              replyingToCommentId={replyingToCommentId}
              expandedReplies={expandedReplies}
              onStartEditComment={onStartEditComment}
              onUpdateComment={onUpdateComment}
              onDeleteComment={onDeleteComment}
              onAddComment={onAddComment}
              setEditingCommentId={setEditingCommentId}
              setEditCommentText={setEditCommentText}
              setCommentDrafts={setCommentDrafts}
              setReplyingToCommentId={setReplyingToCommentId}
              setExpandedReplies={setExpandedReplies}
            />
          ))}
        </div>
      )}
    </div>
  )
}