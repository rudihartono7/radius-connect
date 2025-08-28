import type { ApiResponse, PagedApiResponse } from '~/types'

class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public errors?: string[]
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

function getBaseURL() {
  const config = useRuntimeConfig()
  return config.public.apiBase
}

export const api = {
  get baseURL() {
    return getBaseURL()
  },

  async request<T>(
    endpoint: string,
    options: {
      method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
      body?: string
      headers?: Record<string, string>
    } = {}
  ): Promise<ApiResponse<T>> {
    const url = `${this.baseURL}${endpoint}`
    
    const defaultHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
    }

    // Add auth token if available
    const token = useCookie('auth-token').value
    if (token) {
      defaultHeaders.Authorization = `Bearer ${token}`
    }

    console.log('url: ', url)
    console.log('options: ', options)
    const response = await $fetch<ApiResponse<T>>(url, {
      method: options.method || 'GET',
      body: options.body,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    })

    if (!response.success) {
      throw new ApiError(
        response.message || 'API request failed',
        400,
        response.errors
      )
    }

    return response
  },

  async get<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'GET' })
  },

  async post<T>(endpoint: string, data?: any): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    })
  },

  async put<T>(endpoint: string, data?: any): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    })
  },

  async delete<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'DELETE' })
  },

  async patch<T>(endpoint: string, data?: any): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
    })
  },
}

// Auth API endpoints
export const authApi = {
  login: (data: any) => api.post('/auth/login', data),
  register: (data: any) => api.post('/auth/register', data),
  logout: (data: any) => api.post('/auth/logout', data),
  refresh: (data: any) => api.post('/auth/refresh', data),
  me: () => api.get('/auth/me'),
  changePassword: (data: any) => api.post('/auth/change-password', data),
  forgotPassword: (data: any) => api.post('/auth/forgot-password', data),
  resetPassword: (data: any) => api.post('/auth/reset-password', data),
}

// Users API endpoints
export const usersApi = {
  getUsers: (params?: { page?: number; pageSize?: number; search?: string }) => 
    api.get(`/users${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getUser: (id: string) => api.get(`/users/${id}`),
  createUser: (data: any) => api.post('/users', data),
  updateUser: (id: string, data: any) => api.put(`/users/${id}`, data),
  deleteUser: (id: string) => api.delete(`/users/${id}`),
  activateUser: (id: string) => api.post(`/users/${id}/activate`),
  deactivateUser: (id: string) => api.post(`/users/${id}/deactivate`),
  assignRole: (id: string, data: any) => api.post(`/users/${id}/assign-role`, data),
  removeRole: (id: string, data: any) => api.post(`/users/${id}/remove-role`, data),
  setupTotp: (id: string) => api.get(`/users/${id}/totp/setup`),
  enableTotp: (id: string, data: any) => api.post(`/users/${id}/totp/enable`, data),
  disableTotp: (id: string) => api.post(`/users/${id}/totp/disable`),
}

// Dashboard API endpoints
export const dashboardApi = {
  getOverview: () => api.get('/dashboard/overview'),
  getSystemHealth: () => api.get('/dashboard/system-health'),
  getSessionStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/sessions${params ? '?' + new URLSearchParams(params) : ''}`),
  getHourlySessionStats: (date?: string) => 
    api.get(`/dashboard/sessions/hourly${date ? '?date=' + date : ''}`),
  getDailySessionStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/sessions/daily${params ? '?' + new URLSearchParams(params) : ''}`),
  getAuthenticationStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/authentication${params ? '?' + new URLSearchParams(params) : ''}`),
  getHourlyAuthStats: (date?: string) => 
    api.get(`/dashboard/authentication/hourly${date ? '?date=' + date : ''}`),
  getDailyAuthStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/authentication/daily${params ? '?' + new URLSearchParams(params) : ''}`),
  getUserStats: () => api.get('/dashboard/users'),
  getUserActivityStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/users/activity${params ? '?' + new URLSearchParams(params) : ''}`),
  getTopActiveUsers: (params?: { limit?: number; startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/users/top-active${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getGroupStats: () => api.get('/dashboard/groups'),
  getGroupDistributionStats: () => api.get('/dashboard/groups/distribution'),
  getNetworkStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/network${params ? '?' + new URLSearchParams(params) : ''}`),
  getNasUsageStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/network/nas-usage${params ? '?' + new URLSearchParams(params) : ''}`),
  getBandwidthStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/network/bandwidth${params ? '?' + new URLSearchParams(params) : ''}`),
  getRealTimeActiveSessions: () => api.get('/dashboard/real-time/active-sessions'),
  getRealTimeRecentAuthentications: (limit?: number) => 
    api.get(`/dashboard/real-time/recent-authentications${limit ? '?limit=' + limit : ''}`),
  getRealTimeSystemAlerts: (limit?: number) => 
    api.get(`/dashboard/real-time/system-alerts${limit ? '?limit=' + limit : ''}`),
  getSessionSummaryReport: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/reports/session-summary${params ? '?' + new URLSearchParams(params) : ''}`),
  getAuthenticationSummaryReport: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/reports/authentication-summary${params ? '?' + new URLSearchParams(params) : ''}`),
  getUserActivityReport: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/reports/user-activity${params ? '?' + new URLSearchParams(params) : ''}`),
  getAuditStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/audit${params ? '?' + new URLSearchParams(params) : ''}`),
  getRecentAuditActivities: (limit?: number) => 
    api.get(`/dashboard/audit/recent-activities${limit ? '?limit=' + limit : ''}`),
  getTopAuditActors: (params?: { limit?: number; startDate?: string; endDate?: string }) => 
    api.get(`/dashboard/audit/top-actors${params ? '?' + new URLSearchParams(params as any) : ''}`),
}

// RADIUS API endpoints
export const radiusApi = {
  // Users
  getRadiusUsers: (params?: { page?: number; pageSize?: number; search?: string }) => 
    api.get(`/radius/users${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getRadiusUser: (username: string) => api.get(`/radius/users/${username}`),
  createRadiusUser: (data: any) => api.post('/radius/users', data),
  updateRadiusUser: (username: string, data: any) => api.put(`/radius/users/${username}`, data),
  deleteRadiusUser: (username: string) => api.delete(`/radius/users/${username}`),
  getRadiusUserAttributes: (username: string) => api.get(`/radius/users/${username}/attributes`),
  addRadiusUserAttribute: (username: string, data: any) => api.post(`/radius/users/${username}/attributes`, data),
  removeRadiusUserAttribute: (username: string, attributeId: number) => api.delete(`/radius/users/${username}/attributes/${attributeId}`),
  
  // Check Attributes Management
  getRadiusUserCheckAttributes: (username: string) => api.get(`/radius/users/${username}/check-attributes`),
  addRadiusUserCheckAttribute: (username: string, data: any) => api.post(`/radius/users/${username}/check-attributes`, data),
  updateRadiusUserCheckAttribute: (username: string, attributeId: number, data: any) => api.put(`/radius/users/${username}/check-attributes/${attributeId}`, data),
  removeRadiusUserCheckAttribute: (username: string, attributeId: number) => api.delete(`/radius/users/${username}/check-attributes/${attributeId}`),
  
  // Reply Attributes Management
  getRadiusUserReplyAttributes: (username: string) => api.get(`/radius/users/${username}/reply-attributes`),
  addRadiusUserReplyAttribute: (username: string, data: any) => api.post(`/radius/users/${username}/reply-attributes`, data),
  updateRadiusUserReplyAttribute: (username: string, attributeId: number, data: any) => api.put(`/radius/users/${username}/reply-attributes/${attributeId}`, data),
  removeRadiusUserReplyAttribute: (username: string, attributeId: number) => api.delete(`/radius/users/${username}/reply-attributes/${attributeId}`),
  
  // Groups
  getGroups: (params?: { page?: number; pageSize?: number }) => 
    api.get(`/radius/groups${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getGroup: (groupName: string) => api.get(`/radius/groups/${groupName}`),
  createGroup: (data: any) => api.post('/radius/groups', data),
  updateGroup: (groupName: string, data: any) => api.put(`/radius/groups/${groupName}`, data),
  deleteGroup: (groupName: string) => api.delete(`/radius/groups/${groupName}`),
  addUserToGroup: (groupName: string, username: string, data: any) => api.post(`/radius/groups/${groupName}/users/${username}`, data),
  removeUserFromGroup: (groupName: string, username: string) => api.delete(`/radius/groups/${groupName}/users/${username}`),
  getGroupUsers: (groupName: string) => api.get(`/radius/groups/${groupName}/users`),
  getUserGroups: (username: string) => api.get(`/radius/users/${username}/groups`),
  
  // Sessions
  getActiveSessions: (params?: { page?: number; pageSize?: number }) => 
    api.get(`/radius/sessions${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getSession: (sessionId: string) => api.get(`/radius/sessions/${sessionId}`),
  disconnectSession: (sessionId: string, data: any) => api.post(`/radius/sessions/${sessionId}/disconnect`, data),
  getUserSessions: (username: string, params?: { page?: number; pageSize?: number }) => 
    api.get(`/radius/sessions/user/${username}${params ? '?' + new URLSearchParams(params as any) : ''}`),
  
  // Auth Logs
  getAuthenticationLogs: (params?: { page?: number; pageSize?: number; username?: string; startDate?: string; endDate?: string }) => 
    api.get(`/radius/auth-logs${params ? '?' + new URLSearchParams(params as any) : ''}`),
  
  // Statistics
  getRadiusOverviewStats: () => api.get('/radius/stats/overview'),
  getSessionStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/radius/stats/sessions${params ? '?' + new URLSearchParams(params) : ''}`),
  getAuthenticationStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/radius/stats/authentication${params ? '?' + new URLSearchParams(params) : ''}`),
}

// Audit API endpoints
export const auditApi = {
  getAuditLogs: (params?: { page?: number; pageSize?: number; actor?: string; entity?: string; action?: string; startDate?: string; endDate?: string; search?: string }) => 
    api.get(`/audit/logs${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getAuditLog: (id: string) => api.get(`/audit/logs/${id}`),
  getLogsByActor: (actorId: string, params?: { page?: number; pageSize?: number; startDate?: string; endDate?: string }) => 
    api.get(`/audit/logs/actor/${actorId}${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getLogsByEntity: (entityType: string, params?: { page?: number; pageSize?: number; startDate?: string; endDate?: string }) => 
    api.get(`/audit/logs/entity/${entityType}${params ? '?' + new URLSearchParams(params as any) : ''}`),
  getLogsByDateRange: (params: { startDate: string; endDate: string; page?: number; pageSize?: number }) => 
    api.get(`/audit/logs/date-range?${new URLSearchParams(params as any)}`),
  searchAuditLogs: (params: { query: string; page?: number; pageSize?: number; startDate?: string; endDate?: string }) => 
    api.get(`/audit/search?${new URLSearchParams(params as any)}`),
  getAuditStatistics: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/audit/statistics${params ? '?' + new URLSearchParams(params) : ''}`),
  getLogsByActionStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/audit/statistics/by-action${params ? '?' + new URLSearchParams(params) : ''}`),
  getLogsByEntityStats: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/audit/statistics/by-entity${params ? '?' + new URLSearchParams(params) : ''}`),
  getLogsByDateStats: (params?: { startDate?: string; endDate?: string; groupBy?: string }) => 
    api.get(`/audit/statistics/by-date${params ? '?' + new URLSearchParams(params) : ''}`),
  getMostActiveUsers: (params?: { limit?: number; startDate?: string; endDate?: string }) => 
    api.get(`/audit/statistics/most-active-users${params ? '?' + new URLSearchParams(params as any) : ''}`),
  cleanupOldLogs: (data: any) => api.post('/audit/cleanup', data),
  archiveOldLogs: (data: any) => api.post('/audit/archive', data),
  exportToCsv: (params?: { startDate?: string; endDate?: string; actor?: string; entity?: string; action?: string }) => 
    api.get(`/audit/export/csv${params ? '?' + new URLSearchParams(params) : ''}`),
  exportToJson: (params?: { startDate?: string; endDate?: string; actor?: string; entity?: string; action?: string }) => 
    api.get(`/audit/export/json${params ? '?' + new URLSearchParams(params) : ''}`),
  exportToXml: (params?: { startDate?: string; endDate?: string; actor?: string; entity?: string; action?: string }) => 
    api.get(`/audit/export/xml${params ? '?' + new URLSearchParams(params) : ''}`),
  getAuditSummaryReport: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/audit/reports/summary${params ? '?' + new URLSearchParams(params) : ''}`),
  getDetailedAuditReport: (params?: { startDate?: string; endDate?: string; actor?: string; entity?: string }) => 
    api.get(`/audit/reports/detailed${params ? '?' + new URLSearchParams(params) : ''}`),
  getSecurityAlerts: (limit?: number) => 
    api.get(`/audit/security/alerts${limit ? '?limit=' + limit : ''}`),
  detectAnomalies: (params?: { startDate?: string; endDate?: string }) => 
    api.get(`/audit/security/anomalies${params ? '?' + new URLSearchParams(params) : ''}`),
}
