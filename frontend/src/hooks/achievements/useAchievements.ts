import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '../../config/axios'
import { useApiQuery } from '../useApiQuery'
import type {
  AchievementFeedFilters,
  AchievementPost,
  AchievementPostCreatePayload,
  AchievementPostUpdatePayload,
  AchievementCommentCreatePayload,
  AchievementCommentUpdatePayload
} from '../../types/achievements'

export const useAchievementsFeed = (filters: AchievementFeedFilters) =>
  useApiQuery<AchievementPost[]>(['achievements', filters], '/api/v1/achievements/posts', filters)

export const useCreateAchievementPost = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: AchievementPostCreatePayload | FormData) => {
      const config = payload instanceof FormData ? { headers: { 'Content-Type': 'multipart/form-data' } } : {}
      const response = await apiClient.post<AchievementPost>('/api/v1/achievements/posts', payload, config)
      return response.data?.data ?? response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useUpdateAchievementPost = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ postId, ...payload }: AchievementPostUpdatePayload) => {
      const response = await apiClient.put<AchievementPost>(`/api/v1/achievements/posts/${postId}`, payload)
      return response.data?.data ?? response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useDeleteAchievementPost = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ postId, reason }: { postId: number; reason?: string }) => {
      await apiClient.delete(`/api/v1/achievements/posts/${postId}`, { params: { reason } })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useAddAchievementComment = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ postId, commentText, parentCommentId }: AchievementCommentCreatePayload) => {
      const response = await apiClient.post(`/api/v1/achievements/posts/${postId}/comments`, { commentText, parentCommentId })
      return response.data?.data ?? response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useUpdateAchievementComment = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ commentId, commentText }: AchievementCommentUpdatePayload) => {
      const response = await apiClient.put(`/api/v1/achievements/comments/${commentId}`, { commentText })
      return response.data?.data ?? response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useDeleteAchievementComment = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ commentId, reason }: { commentId: number; reason?: string }) => {
      await apiClient.delete(`/api/v1/achievements/comments/${commentId}`, { params: { reason } })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useLikeAchievementPost = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (postId: number) => {
      await apiClient.post(`/api/v1/achievements/posts/${postId}/likes`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}

export const useUnlikeAchievementPost = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (postId: number) => {
      await apiClient.delete(`/api/v1/achievements/posts/${postId}/likes`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] })
    }
  })
}