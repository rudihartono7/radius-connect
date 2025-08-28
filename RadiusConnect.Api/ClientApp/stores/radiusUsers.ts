import { defineStore } from 'pinia'
import type { RadiusUser, CreateRadiusUserRequest, UpdateRadiusUserRequest, RadiusAttribute, RadiusAttributeDto } from '~/types'
import { radiusApi } from '~/utils/api'

interface RadiusUsersState {
  users: RadiusUser[]
  loading: boolean
  error: string | null
  currentPage: number
  pageSize: number
  totalCount: number
  totalPages: number
}

interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

export const useRadiusUsersStore = defineStore('radiusUsers', {
  state: (): RadiusUsersState => ({
    users: [],
    loading: false,
    error: null,
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  }),

  actions: {
    clearError() {
      this.error = null
    },

    setError(error: string) {
      this.error = error
      this.loading = false
    },

    async fetchUsers(page: number = 1, pageSize: number = 10, search?: string) {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.getRadiusUsers({
          page,
          pageSize,
          search
        })

        if (result.success && result.data) {
          // Handle both paginated and non-paginated responses
          if (Array.isArray(result.data)) {
            this.users = result.data as RadiusUser[]
            this.totalCount = result.data.length
            this.currentPage = page
            this.totalPages = Math.ceil(result.data.length / pageSize)
          } else {
            const data = result.data as any
            this.users = data.users || data
            this.totalCount = data.totalCount || data.length || 0
            this.currentPage = data.currentPage || page
            this.totalPages = data.totalPages || Math.ceil(this.totalCount / pageSize)
          }
          this.pageSize = pageSize
        } else {
          throw new Error(result.message || 'Failed to fetch RADIUS users')
        }
      } catch (error) {
        console.error('Error fetching RADIUS users:', error)
        this.setError(error instanceof Error ? error.message : 'Failed to fetch RADIUS users')
      } finally {
        this.loading = false
      }
    },

    async fetchUser(username: string): Promise<RadiusUser | null> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.getRadiusUser(username)

        if (result.success && result.data) {
          return result.data as RadiusUser
        } else {
          throw new Error(result.message || 'Failed to fetch RADIUS user')
        }
      } catch (error) {
        console.error('Error fetching RADIUS user:', error)
        this.setError(error instanceof Error ? error.message : 'Failed to fetch RADIUS user')
        return null
      } finally {
        this.loading = false
      }
    },

    async createUser(userData: CreateRadiusUserRequest): Promise<ApiResponse<RadiusUser>> {
      this.loading = true
      this.error = null

      try {
        // Ensure userData has the required structure for backend
        const createRequest: CreateRadiusUserRequest = {
          username: userData.username,
          password: userData.password,
          checkAttributes: userData.checkAttributes || [],
          replyAttributes: userData.replyAttributes || [],
          groups: userData.groups || []
        }

        const result = await radiusApi.createRadiusUser(createRequest)

        if (result.success) {
          // Add the new user to the local state
          if (result.data) {
            this.users.unshift(result.data as RadiusUser)
            this.totalCount++
          }
        } else {
          throw new Error(result.message || 'Failed to create RADIUS user')
        }

        return result as ApiResponse<RadiusUser>
      } catch (error) {
        console.error('Error creating RADIUS user:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to create RADIUS user'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async updateUser(username: string, userData: UpdateRadiusUserRequest): Promise<ApiResponse<RadiusUser>> {
      this.loading = true
      this.error = null

      try {
        // Ensure userData has the required structure for backend
        const updateRequest: UpdateRadiusUserRequest = {
          password: userData.password,
          checkAttributes: userData.checkAttributes || [],
          replyAttributes: userData.replyAttributes || [],
          groups: userData.groups || []
        }

        const result = await radiusApi.updateRadiusUser(username, updateRequest)

        if (result.success && result.data) {
          // Update the user in the local state
          const index = this.users.findIndex(u => u.username === username)
          if (index !== -1) {
            this.users[index] = result.data as RadiusUser
          }
        } else {
          throw new Error(result.message || 'Failed to update RADIUS user')
        }

        return result as ApiResponse<RadiusUser>
      } catch (error) {
        console.error('Error updating RADIUS user:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to update RADIUS user'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async deleteUser(username: string): Promise<ApiResponse<void>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.deleteRadiusUser(username)

        if (result.success) {
          // Remove the user from the local state
          this.users = this.users.filter(u => u.username !== username)
          this.totalCount--
        } else {
          throw new Error(result.message || 'Failed to delete RADIUS user')
        }

        return result as ApiResponse<void>
      } catch (error) {
        console.error('Error deleting RADIUS user:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to delete RADIUS user'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async getUserAttributes(username: string) {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.getRadiusUserAttributes(username)
        return result
      } catch (error) {
        console.error('Error fetching user attributes:', error)
        this.setError(error instanceof Error ? error.message : 'Failed to fetch user attributes')
        return null
      } finally {
        this.loading = false
      }
    },

    async addUserAttribute(username: string, attribute: { name: string; value: string; type: 'Check' | 'Reply' }) {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.addRadiusUserAttribute(username, attribute)

        return result
      } catch (error) {
        console.error('Error adding user attribute:', error)
        this.setError(error instanceof Error ? error.message : 'Failed to add user attribute')
        return { success: false }
      } finally {
        this.loading = false
      }
    },

    async removeUserAttribute(username: string, attributeName: string) {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.removeRadiusUserAttribute(username, parseInt(attributeName))

        return result
      } catch (error) {
        console.error('Error removing user attribute:', error)
        this.setError(error instanceof Error ? error.message : 'Failed to remove user attribute')
        return { success: false }
      } finally {
        this.loading = false
      }
    },

    // Attribute management
    async updateUserAttributes(username: string, checkAttributes: RadiusAttribute[], replyAttributes: RadiusAttribute[]) {
      this.loading = true
      this.error = null

      try {
        // Convert RadiusAttribute[] to RadiusAttributeDto[] for backend
        const checkAttributesDto: RadiusAttributeDto[] = checkAttributes.map(attr => ({
          attribute: attr.attribute,
          op: attr.op,
          value: attr.value
        }))
        
        const replyAttributesDto: RadiusAttributeDto[] = replyAttributes.map(attr => ({
          attribute: attr.attribute,
          op: attr.op,
          value: attr.value
        }))

        const updateRequest: UpdateRadiusUserRequest = {
          checkAttributes: checkAttributesDto,
          replyAttributes: replyAttributesDto,
          groups: [] // Initialize with empty array as required by backend
        }
        
        const result = await radiusApi.updateRadiusUser(username, updateRequest)
        
        if (result.success) {
           // Update the user in the store
           const userIndex = this.users.findIndex(u => u.username === username)
           if (userIndex !== -1 && this.users[userIndex]) {
             this.users[userIndex].checkAttributes = checkAttributes
             this.users[userIndex].replyAttributes = replyAttributes
           }
         } else {
          this.error = result.message || 'Failed to update user attributes'
        }
        
        return result
      } catch (error) {
        console.error('Error updating user attributes:', error)
        this.error = error instanceof Error ? error.message : 'Failed to update user attributes'
        return { success: false, message: this.error }
      } finally {
        this.loading = false
      }
    },

    // Check Attributes Management
    async getUserCheckAttributes(username: string): Promise<ApiResponse<RadiusAttribute[]>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.getRadiusUserCheckAttributes(username)
        return result as ApiResponse<RadiusAttribute[]>
      } catch (error) {
        console.error('Error fetching user check attributes:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to fetch check attributes'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async addUserCheckAttribute(username: string, attribute: RadiusAttributeDto): Promise<ApiResponse<void>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.addRadiusUserCheckAttribute(username, attribute)
        
        if (result.success) {
          // Refresh user data to get updated attributes
          await this.fetchUser(username)
        }
        
        return result as ApiResponse<void>
      } catch (error) {
        console.error('Error adding user check attribute:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to add check attribute'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async removeUserCheckAttribute(username: string, attributeId: number): Promise<ApiResponse<void>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.removeRadiusUserCheckAttribute(username, attributeId)
        
        if (result.success) {
          // Refresh user data to get updated attributes
          await this.fetchUser(username)
        }
        
        return result as ApiResponse<void>
      } catch (error) {
        console.error('Error removing user check attribute:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to remove check attribute'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    // Reply Attributes Management
    async getUserReplyAttributes(username: string): Promise<ApiResponse<RadiusAttribute[]>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.getRadiusUserReplyAttributes(username)
        return result as ApiResponse<RadiusAttribute[]>
      } catch (error) {
        console.error('Error fetching user reply attributes:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to fetch reply attributes'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async addUserReplyAttribute(username: string, attribute: RadiusAttributeDto): Promise<ApiResponse<void>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.addRadiusUserReplyAttribute(username, attribute)
        
        if (result.success) {
          // Refresh user data to get updated attributes
          await this.fetchUser(username)
        }
        
        return result as ApiResponse<void>
      } catch (error) {
        console.error('Error adding user reply attribute:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to add reply attribute'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    },

    async removeUserReplyAttribute(username: string, attributeId: number): Promise<ApiResponse<void>> {
      this.loading = true
      this.error = null

      try {
        const result = await radiusApi.removeRadiusUserReplyAttribute(username, attributeId)
        
        if (result.success) {
          // Refresh user data to get updated attributes
          await this.fetchUser(username)
        }
        
        return result as ApiResponse<void>
      } catch (error) {
        console.error('Error removing user reply attribute:', error)
        const errorMessage = error instanceof Error ? error.message : 'Failed to remove reply attribute'
        this.setError(errorMessage)
        return {
          success: false,
          message: errorMessage
        }
      } finally {
        this.loading = false
      }
    }
  }
})