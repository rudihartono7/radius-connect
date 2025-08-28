// API Response Types
export interface ApiResponse<T = any> {
  success: boolean
  message: string
  data: T
  errors?: string[]
}

export interface PagedApiResponse<T = any> extends ApiResponse<T> {
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// User Types
export interface User {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
  isActive: boolean
  isTotpEnabled: boolean
  lastLogin?: string
  createdAt: string
  updatedAt?: string
  roles: string[]
}

export interface CreateUserRequest {
  username: string
  email: string
  password: string
  firstName?: string
  lastName?: string
}

export interface UpdateUserRequest {
  username: string
  email: string
  firstName?: string
  lastName?: string
}

// Auth Types
export interface LoginRequest {
  username: string
  password: string
  totpCode?: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  firstName?: string
  lastName?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  resetToken: string
  newPassword: string
}

// Dashboard Types
export interface DashboardOverview {
  totalUsers: number
  activeSessions: number
  totalGroups: number
  totalAuthentications: number
  successRate: number
  averageSessionDuration: number
}

export interface SystemHealth {
  databaseStatus: string
  radiusServerStatus: string
  diskUsage: number
  memoryUsage: number
  cpuUsage: number
}

export interface SessionStats {
  totalSessions: number
  activeSessions: number
  averageDuration: number
  peakConcurrent: number
}

export interface AuthenticationStats {
  totalAttempts: number
  successfulAttempts: number
  failedAttempts: number
  successRate: number
}

// RADIUS Types
export interface RadiusAttribute {
  attribute: string
  op: string
  value: string
}

export interface RadiusAttributeDto {
  attribute: string
  op: string
  value: string
}

export interface RadiusUser {
  username: string
  isActive: boolean
  lastAuth?: string
  checkAttributes: RadiusAttribute[]
  replyAttributes: RadiusAttribute[]
  groups: string[]
}

export interface RadiusGroup {
  groupName: string
  description?: string
  priority: number
  checkAttributes: RadiusAttribute[]
  replyAttributes: RadiusAttribute[]
  users: string[]
}

// Backend DTO types (matching C# DTOs)
export interface CreateRadiusUserRequest {
  username: string
  password: string
  checkAttributes: RadiusAttributeDto[]
  replyAttributes: RadiusAttributeDto[]
  groups: string[]
}

export interface UpdateRadiusUserRequest {
  password?: string
  checkAttributes: RadiusAttributeDto[]
  replyAttributes: RadiusAttributeDto[]
  groups: string[]
}

export interface CreateRadiusGroupRequest {
  groupName: string
  description?: string
  priority?: number
  checkAttributes?: RadiusAttributeDto[]
  replyAttributes?: RadiusAttributeDto[]
}

export interface UpdateRadiusGroupRequest {
  description?: string
  priority?: number
  checkAttributes?: RadiusAttributeDto[]
  replyAttributes?: RadiusAttributeDto[]
}

// Legacy types for backward compatibility (using Dictionary structure)
export interface CreateRadiusUserRequestLegacy {
  username: string
  password: string
  attributes?: Record<string, string>
}

export interface UpdateRadiusUserRequestLegacy {
  password?: string
  attributes?: Record<string, string>
}

export interface AddUserToGroupRequest {
  priority?: number
}

// Session Types
export interface RadiusSession {
  sessionId: string
  username: string
  nasIpAddress: string
  startTime: string
  duration: number
  inputOctets: number
  outputOctets: number
  status: string
}

export interface DisconnectSessionRequest {
  reason: string
}

// Audit Types
export interface AuditLog {
  id: string
  timestamp: string
  actorId: string
  actorName: string
  action: string
  entityType: string
  entityId: string
  details: string
  ipAddress?: string
  userAgent?: string
}

export interface AuditStatistics {
  totalLogs: number
  logsByAction: Record<string, number>
  logsByEntity: Record<string, number>
  logsByDate: Array<{ date: string; count: number }>
  mostActiveUsers: Array<{ username: string; actionCount: number }>
}

// Chart Data Types
export interface ChartDataPoint {
  label: string
  value: number
}

export interface TimeSeriesData {
  labels: string[]
  datasets: Array<{
    label: string
    data: number[]
    borderColor?: string
    backgroundColor?: string
  }>
}
