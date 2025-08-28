import { defineStore } from 'pinia'
import type { PagedApiResponse } from '~/types'
import { radiusApi } from '~/utils/api'

interface RadiusSession {
  sessionId: string
  username: string
  nasIpAddress: string
  nasPort: number
  sessionTime: number
  inputOctets: number
  outputOctets: number
  startTime: string
  lastUpdate: string
  isActive: boolean
  acctSessionId?: string
  framedIpAddress?: string
  calledStationId?: string
  callingStationId?: string
}

interface DisconnectSessionRequest {
  reason: string
}

interface SessionsState {
  sessions: RadiusSession[]
  currentSession: RadiusSession | null
  userSessions: RadiusSession[]
  loading: boolean
  error: string | null
  pagination: {
    page: number
    pageSize: number
    total: number
    totalPages: number
  }
}

export const useSessionsStore = defineStore('sessions', {
  state: (): SessionsState => ({
    sessions: [],
    currentSession: null,
    userSessions: [],
    loading: false,
    error: null,
    pagination: {
      page: 1,
      pageSize: 10,
      total: 0,
      totalPages: 0
    }
  }),

  getters: {
    hasSessions: (state) => state.sessions.length > 0,
    activeSessions: (state) => state.sessions.filter(session => session.isActive),
    currentSessionId: (state) => state.currentSession?.sessionId,
  },

  actions: {
    async fetchActiveSessions(params?: { page?: number; pageSize?: number }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || this.pagination.page
        const pageSize = params?.pageSize || this.pagination.pageSize
        
        const response = await radiusApi.getActiveSessions({ page, pageSize })
        const data = response.data as PagedApiResponse<RadiusSession[]>
        
        this.sessions = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch active sessions'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchSession(sessionId: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await radiusApi.getSession(sessionId)
        this.currentSession = response.data as RadiusSession
        return { success: true, data: this.currentSession }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch session'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async disconnectSession(sessionId: string, reason: string) {
      this.loading = true
      this.error = null
      
      try {
        const request: DisconnectSessionRequest = { reason }
        await radiusApi.disconnectSession(sessionId, request)
        await this.fetchActiveSessions() // Refresh the list
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to disconnect session'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchUserSessions(username: string, params?: { page?: number; pageSize?: number }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || 1
        const pageSize = params?.pageSize || 10
        
        const response = await radiusApi.getUserSessions(username, { page, pageSize })
        const data = response.data as PagedApiResponse<RadiusSession[]>
        
        this.userSessions = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch user sessions'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },

    clearCurrentSession() {
      this.currentSession = null
    },

    setPage(page: number) {
      this.pagination.page = page
    },

    setPageSize(pageSize: number) {
      this.pagination.pageSize = pageSize
      this.pagination.page = 1 // Reset to first page
    }
  }
})
