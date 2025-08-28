import { defineStore } from 'pinia'
import type { PagedApiResponse } from '~/types'
import { auditApi } from '~/utils/api'

interface AuditLog {
  id: string
  timestamp: string
  actorId: string
  actorName: string
  action: string
  entityType: string
  entityId: string
  details: string
  ipAddress: string
  userAgent: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  category: string
  oldValues?: any
  newValues?: any
}

interface AuditStatistics {
  totalLogs: number
  logsByAction: Record<string, number>
  logsByEntity: Record<string, number>
  logsByDate: Record<string, number>
  mostActiveUsers: Array<{ username: string; actionCount: number }>
  securityAlerts: number
  anomalies: number
}

interface AuditFilters {
  actor?: string
  entity?: string
  action?: string
  startDate?: string
  endDate?: string
  search?: string
  severity?: string
}

interface AuditState {
  logs: AuditLog[]
  currentLog: AuditLog | null
  statistics: AuditStatistics | null
  loading: boolean
  error: string | null
  filters: AuditFilters
  pagination: {
    page: number
    pageSize: number
    total: number
    totalPages: number
  }
}

export const useAuditStore = defineStore('audit', {
  state: (): AuditState => ({
    logs: [],
    currentLog: null,
    statistics: null,
    loading: false,
    error: null,
    filters: {},
    pagination: {
      page: 1,
      pageSize: 20,
      total: 0,
      totalPages: 0
    }
  }),

  getters: {
    hasLogs: (state) => state.logs.length > 0,
    filteredLogs: (state) => state.logs,
    currentLogId: (state) => state.currentLog?.id,
  },

  actions: {
    async fetchAuditLogs(params?: { page?: number; pageSize?: number; filters?: AuditFilters }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || this.pagination.page
        const pageSize = params?.pageSize || this.pagination.pageSize
        const filters = params?.filters || this.filters
        
        const queryParams = {
          page,
          pageSize,
          ...filters
        }
        
        const response = await auditApi.getAuditLogs(queryParams)
        
        const data = response as PagedApiResponse<AuditLog[]>
        
        this.logs = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch audit logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchAuditLog(id: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await auditApi.getAuditLog(id)
        this.currentLog = response.data as AuditLog
        return { success: true, data: this.currentLog }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch audit log'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchLogsByActor(actorId: string, params?: { page?: number; pageSize?: number; startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || 1
        const pageSize = params?.pageSize || 20
        
        const response = await auditApi.getLogsByActor(actorId, { page, pageSize, startDate: params?.startDate, endDate: params?.endDate })
        const data = response.data as PagedApiResponse<AuditLog[]>
        
        this.logs = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch actor logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchLogsByEntity(entityType: string, params?: { page?: number; pageSize?: number; startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || 1
        const pageSize = params?.pageSize || 20
        
        const response = await auditApi.getLogsByEntity(entityType, { page, pageSize, startDate: params?.startDate, endDate: params?.endDate })
        const data = response.data as PagedApiResponse<AuditLog[]>
        
        this.logs = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch entity logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async searchAuditLogs(query: string, params?: { page?: number; pageSize?: number; startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || 1
        const pageSize = params?.pageSize || 20
        
        const response = await auditApi.searchAuditLogs({ query, page, pageSize, startDate: params?.startDate, endDate: params?.endDate })
        const data = response.data as PagedApiResponse<AuditLog[]>
        
        this.logs = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to search audit logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchAuditStatistics(params?: { startDate?: string; endDate?: string }) {
      this.loading = true
      this.error = null
      
      try {
        const response = await auditApi.getAuditStatistics(params)
        this.statistics = response.data as AuditStatistics
        return { success: true, data: this.statistics }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch audit statistics'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async exportToCsv(params?: { startDate?: string; endDate?: string; actor?: string; entity?: string; action?: string }) {
      try {
        const response = await auditApi.exportToCsv(params)
        // Handle file download
        const blob = new Blob([response.data], { type: 'text/csv' })
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `audit_logs_${new Date().toISOString().split('T')[0]}.csv`
        document.body.appendChild(a)
        a.click()
        window.URL.revokeObjectURL(url)
        document.body.removeChild(a)
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to export to CSV'
        return { success: false, error: this.error }
      }
    },

    async exportToJson(params?: { startDate?: string; endDate?: string; actor?: string; entity?: string; action?: string }) {
      try {
        const response = await auditApi.exportToJson(params)
        // Handle file download
        const blob = new Blob([response.data], { type: 'application/json' })
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `audit_logs_${new Date().toISOString().split('T')[0]}.json`
        document.body.appendChild(a)
        a.click()
        window.URL.revokeObjectURL(url)
        document.body.removeChild(a)
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to export to JSON'
        return { success: false, error: this.error }
      }
    },

    async cleanupOldLogs(olderThanDays: number) {
      this.loading = true
      this.error = null
      
      try {
        const response = await auditApi.cleanupOldLogs({ olderThanDays })
        return { success: true, data: response.data }
      } catch (error: any) {
        this.error = error.message || 'Failed to cleanup old logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async archiveOldLogs(olderThanDays: number, archivePath: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await auditApi.archiveOldLogs({ olderThanDays, archivePath })
        return { success: true, data: response.data }
      } catch (error: any) {
        this.error = error.message || 'Failed to archive old logs'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    setFilters(filters: AuditFilters) {
      this.filters = { ...this.filters, ...filters }
      this.pagination.page = 1 // Reset to first page when filters change
    },

    clearFilters() {
      this.filters = {}
      this.pagination.page = 1
    },

    clearError() {
      this.error = null
    },

    clearCurrentLog() {
      this.currentLog = null
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
