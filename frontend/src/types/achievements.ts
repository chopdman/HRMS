export type AchievementUser = {
  id: number
  fullName: string
  profilePhotoUrl?: string | null
}

export type AchievementComment = {
  commentId: number
  postId: number
  author: AchievementUser
  commentText: string
  createdAt: string
  updatedAt: string
  parentCommentId?: number | null
  replies?: AchievementComment[] | null
}

export type AchievementPost = {
  postId: number
  author: AchievementUser
  title: string
  description?: string | null
  tags: string[]
  postType: 'Achievement' | 'Birthday' | 'WorkAnniversary'
  visibility: 'AllEmployees'
  isSystemGenerated: boolean
  createdAt: string
  updatedAt: string
  likeCount: number
  hasLiked: boolean
  recentLikers: AchievementUser[]
  commentCount: number
  comments: AchievementComment[]
  attachmentUrl?: string | null
  attachmentFileName?: string | null
}

export type AchievementFeedFilters = {
  author?: string
  authorId?: number
  tag?: string
  fromDate?: string
  toDate?: string
}

export type AchievementPostCreatePayload = {
  title: string
  description?: string
  tags?: string[]
  visibility?: 'AllEmployees'
}

export type AchievementPostUpdatePayload = AchievementPostCreatePayload & {
  postId: number
}

export type AchievementCommentCreatePayload = {
  postId: number
  commentText: string
  parentCommentId?: number | null
}

export type AchievementCommentUpdatePayload = {
  commentId: number
  commentText: string
}