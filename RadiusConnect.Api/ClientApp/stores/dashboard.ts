import { defineStore } from 'pinia'
import type { DashboardOverview, SystemHealth, SessionStats, AuthenticationStats } from '~/types'
import { dashboardApi } from '~/utils/api'

interface DashboardState {
  overview: DashboardOverview | null
  systemHealth: SystemHealth | null
  sessionStats: SessionStats | null
  authStats: AuthenticationStats | null
  loading: boolean
  error: string | null
}

export const useDashboardStore = defineStore('dashboard', {
  state: (): DashboardState => ({
    overview: null,
    systemHealth: null,
    sessionStats: null,
    authStats: null,
    loading: false,
    error: null,
  }),

  getters: {
    hasData: (state) => !!(state.overview || state.systemHealth || state.sessionStats || state.authStats),
  },

  actions: {
    async fetchOverview() {
      this.loading = true
      this.error = null
      try {
        const response = await dashboardApi.getOverview()
        this.overview = response.data as DashboardOverview
        return { success: true, data: this.overview }
      } catch (error: any) {
        this.error = error.message
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async fetchSystemHealth() {
      this.loading = true
      this.error = null
      try {
        const response = await dashboardApi.getSystemHealth()
        this.systemHealth = response.data as SystemHealth
        return { success: true, data: this.systemHealth }
      } catch (error: any) {
        this.error = error.message
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async fetchSessionStats(params?: { startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      try {
        const response = await dashboardApi.getSessionStats(params)
        this.sessionStats = response.data as SessionStats
        return { success: true, data: this.sessionStats }
      } catch (error: any) {
        this.error = error.message
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async fetchAuthStats(params?: { startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      try {
        const response = await dashboardApi.getAuthenticationStats(params)
        this.authStats = response.data as AuthenticationStats
        return { success: true, data: this.authStats }
      } catch (error: any) {
        this.error = error.message
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async fetchAllData(params?: { startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      try {
        const [overviewRes, healthRes, sessionRes, authRes] = await Promise.all([
          dashboardApi.getOverview(),
          dashboardApi.getSystemHealth(),
          dashboardApi.getSessionStats(params),
          dashboardApi.getAuthenticationStats(params),
        ])

        this.overview = overviewRes.data as DashboardOverview
        this.systemHealth = healthRes.data as SystemHealth
        this.sessionStats = sessionRes.data as SessionStats
        this.authStats = authRes.data as AuthenticationStats

        return { success: true }
      } catch (error: any) {
        this.error = error.message
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },

    clearData() {
      this.overview = null
      this.systemHealth = null
      this.sessionStats = null
      this.authStats = null
      this.error = null
    },
  },
})
