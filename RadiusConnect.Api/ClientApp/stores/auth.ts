import { defineStore } from 'pinia'
import type { User, LoginRequest, LoginResponse, RegisterRequest } from '~/types'
import { authApi } from '~/utils/api'

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  loading: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: null,
    isAuthenticated: false,
    loading: false,
  }),

  getters: {
    currentUser: (state) => state.user,
    isAdmin: (state) => state.user?.roles.includes('Admin') ?? false,
    isManager: (state) => state.user?.roles.includes('Manager') ?? false,
    hasRole: (state) => (role: string) => state.user?.roles.includes(role) ?? false,
  },

  actions: {
    async login(credentials: LoginRequest) {
      this.loading = true
      try {
        const response = await authApi.login(credentials)
        const data = response.data as LoginResponse
        
        this.token = data.accessToken
        this.user = data.user
        this.isAuthenticated = true
        
        // Store tokens using composable
        const { setToken, setRefreshToken } = useAuthCookies()
        setToken(data.accessToken)
        setRefreshToken(data.refreshToken)
        
        return { success: true, data }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async register(userData: RegisterRequest) {
      this.loading = true
      try {
        const response = await authApi.register(userData)
        return { success: true, data: response.data }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async logout() {
      try {
        const { getRefreshToken } = useAuthCookies()
        const refreshToken = getRefreshToken()
        if (refreshToken) {
          await authApi.logout({ refreshToken })
        }
      } catch (error) {
        console.error('Logout error:', error)
      } finally {
        this.clearAuth()
      }
    },

    async refreshToken() {
      try {
        const { getRefreshToken, setToken } = useAuthCookies()
        const refreshToken = getRefreshToken()
        if (!refreshToken) {
          throw new Error('No refresh token available')
        }

        const response = await authApi.refresh({ refreshToken })
        const data = response.data as LoginResponse
        
        this.token = data.accessToken
        this.user = data.user
        this.isAuthenticated = true
        
        // Update token cookie
        setToken(data.accessToken)
        
        return { success: true, data }
      } catch (error: any) {
        this.clearAuth()
        return { success: false, error: error.message }
      }
    },

    async getCurrentUser() {
      try {
        const response = await authApi.me()
        this.user = response.data as User
        this.isAuthenticated = true
        return { success: true, user: this.user }
      } catch (error: any) {
        // Only clear auth if it's an authentication error (401)
        if (error.status === 401) {
          this.clearAuth()
        }
        return { success: false, error: error.message }
      }
    },

    async changePassword(data: { currentPassword: string; newPassword: string }) {
      try {
        await authApi.changePassword(data)
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async forgotPassword(email: string) {
      try {
        await authApi.forgotPassword({ email })
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async resetPassword(data: { email: string; resetToken: string; newPassword: string }) {
      try {
        await authApi.resetPassword(data)
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    clearAuth() {
      this.user = null
      this.token = null
      this.isAuthenticated = false
      
      // Clear cookies using composable
      const { clearTokens } = useAuthCookies()
      clearTokens()
    },

    async initializeAuth() {
      const { getToken } = useAuthCookies()
      const token = getToken()
      if (token) {
        this.token = token
        // Don't set isAuthenticated until we successfully get user data
        // Try to get current user
        try {
          const result = await this.getCurrentUser()
          if (result.success) {
            this.isAuthenticated = true
          } else {
            // If getCurrentUser fails, try to refresh the token
            const refreshResult = await this.refreshToken()
            if (!refreshResult.success) {
              this.clearAuth()
            }
          }
        } catch (error) {
          // If getCurrentUser fails, try to refresh the token
          try {
            const refreshResult = await this.refreshToken()
            if (!refreshResult.success) {
              this.clearAuth()
            }
          } catch (refreshError) {
            this.clearAuth()
          }
        }
      }
    },
  },
})
