import { defineStore } from 'pinia'
import type { User, CreateUserRequest, UpdateUserRequest, PagedApiResponse } from '~/types'
import { usersApi } from '~/utils/api'

interface UsersState {
  users: User[]
  currentUser: User | null
  loading: boolean
  totalCount: number
  currentPage: number
  pageSize: number
  totalPages: number
}

export const useUsersStore = defineStore('users', {
  state: (): UsersState => ({
    users: [],
    currentUser: null,
    loading: false,
    totalCount: 0,
    currentPage: 1,
    pageSize: 10,
    totalPages: 0,
  }),

  getters: {
    activeUsers: (state) => state.users.filter(user => user.isActive),
    inactiveUsers: (state) => state.users.filter(user => !user.isActive),
    usersByRole: (state) => (role: string) => 
      state.users.filter(user => user.roles.includes(role)),
  },

  actions: {
    async fetchUsers(params?: { page?: number; pageSize?: number; search?: string }) {
      this.loading = true
      try {
        if(params == undefined){
          params = {
            page: 1,
            pageSize: 10,
            search: '',
          }
        }

        if(params.page == undefined){
          params.page = 1
        }

        if(params.pageSize == undefined){
          params.pageSize = 10
        }

        if(params.search == undefined){
          params.search = ''
        }

        console.log('fetchUsers params', params)

        const response = await usersApi.getUsers(params)

        console.log('response: ', response)

        // Backend returns PagedApiResponse<T> where data contains the array directly
        const pagedResponse = response as PagedApiResponse<User[]>

        this.users = pagedResponse.data as User[]
        this.totalCount = pagedResponse.totalCount
        this.currentPage = pagedResponse.page
        this.pageSize = pagedResponse.pageSize
        this.totalPages = pagedResponse.totalPages
        
        return { success: true, data: pagedResponse }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async fetchUser(id: string) {
      this.loading = true
      try {
        const response = await usersApi.getUser(id)
        this.currentUser = response.data as User
        return { success: true, user: this.currentUser }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async createUser(userData: CreateUserRequest) {
      this.loading = true
      try {
        const response = await usersApi.createUser(userData)
        const newUser = response.data as User
        this.users.unshift(newUser)
        this.totalCount++
        return { success: true, user: newUser }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async updateUser(id: string, userData: UpdateUserRequest) {
      this.loading = true
      try {
        const response = await usersApi.updateUser(id, userData)
        const updatedUser = response.data as User
        
        // Update in users array
        const index = this.users.findIndex(user => user.id === id)
        if (index !== -1) {
          this.users[index] = updatedUser
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === id) {
          this.currentUser = updatedUser
        }
        
        return { success: true, user: updatedUser }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async deleteUser(id: string) {
      this.loading = true
      try {
        await usersApi.deleteUser(id)
        
        // Remove from users array
        this.users = this.users.filter(user => user.id !== id)
        this.totalCount--
        
        // Clear current user if it's the same
        if (this.currentUser?.id === id) {
          this.currentUser = null
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      } finally {
        this.loading = false
      }
    },

    async activateUser(id: string) {
      try {
        await usersApi.activateUser(id)
        
        // Update user status in array
        const user = this.users.find(u => u.id === id)
        if (user) {
          user.isActive = true
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === id) {
          this.currentUser.isActive = true
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async deactivateUser(id: string) {
      try {
        await usersApi.deactivateUser(id)
        
        // Update user status in array
        const user = this.users.find(u => u.id === id)
        if (user) {
          user.isActive = false
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === id) {
          this.currentUser.isActive = false
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async assignRole(userId: string, roleName: string) {
      try {
        await usersApi.assignRole(userId, { roleName })
        
        // Update user roles in array
        const user = this.users.find(u => u.id === userId)
        if (user && !user.roles.includes(roleName)) {
          user.roles.push(roleName)
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === userId && !this.currentUser.roles.includes(roleName)) {
          this.currentUser.roles.push(roleName)
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async removeRole(userId: string, roleName: string) {
      try {
        await usersApi.removeRole(userId, { roleName })
        
        // Update user roles in array
        const user = this.users.find(u => u.id === userId)
        if (user) {
          user.roles = user.roles.filter(role => role !== roleName)
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === userId) {
          this.currentUser.roles = this.currentUser.roles.filter(role => role !== roleName)
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async setupTotp(userId: string) {
      try {
        const response = await usersApi.setupTotp(userId)
        return { success: true, data: response.data }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async enableTotp(userId: string, totpCode: string) {
      try {
        await usersApi.enableTotp(userId, { totpCode })
        
        // Update user TOTP status in array
        const user = this.users.find(u => u.id === userId)
        if (user) {
          user.isTotpEnabled = true
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === userId) {
          this.currentUser.isTotpEnabled = true
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    async disableTotp(userId: string) {
      try {
        await usersApi.disableTotp(userId)
        
        // Update user TOTP status in array
        const user = this.users.find(u => u.id === userId)
        if (user) {
          user.isTotpEnabled = false
        }
        
        // Update current user if it's the same
        if (this.currentUser?.id === userId) {
          this.currentUser.isTotpEnabled = false
        }
        
        return { success: true }
      } catch (error: any) {
        return { success: false, error: error.message }
      }
    },

    clearCurrentUser() {
      this.currentUser = null
    },

    setPage(page: number) {
      this.currentPage = page
    },

    setPageSize(pageSize: number) {
      this.pageSize = pageSize
      this.currentPage = 1 // Reset to first page
    },
  },
})
