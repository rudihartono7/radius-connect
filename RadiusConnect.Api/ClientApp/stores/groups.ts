import { defineStore } from 'pinia'
import type { PagedApiResponse } from '~/types'
import { radiusApi } from '~/utils/api'

interface RadiusGroup {
  groupName: string
  checkAttributes?: Record<string, string>
  replyAttributes?: Record<string, string>
  userCount?: number
  isActive?: boolean
}

interface CreateGroupRequest {
  groupName: string
  checkAttributes?: Record<string, string>
  replyAttributes?: Record<string, string>
}

interface AddUserToGroupRequest {
  priority?: number
}

interface GroupsState {
  groups: RadiusGroup[]
  currentGroup: RadiusGroup | null
  groupUsers: any[]
  userGroups: any[]
  loading: boolean
  error: string | null
  pagination: {
    page: number
    pageSize: number
    total: number
    totalPages: number
  }
}

export const useGroupsStore = defineStore('groups', {
  state: (): GroupsState => ({
    groups: [],
    currentGroup: null,
    groupUsers: [],
    userGroups: [],
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
    hasGroups: (state) => state.groups.length > 0,
    currentGroupName: (state) => state.currentGroup?.groupName,
  },

  actions: {
    async fetchGroups(params?: { page?: number; pageSize?: number }) {
      this.loading = true
      this.error = null
      
      try {
        const page = params?.page || this.pagination.page
        const pageSize = params?.pageSize || this.pagination.pageSize
        
        const response = await radiusApi.getGroups(page, pageSize)
        const data = response.data as PagedApiResponse<RadiusGroup[]>
        
        this.groups = data.data || []
        this.pagination = {
          page: data.pagination?.page || page,
          pageSize: data.pagination?.pageSize || pageSize,
          total: data.pagination?.total || 0,
          totalPages: data.pagination?.totalPages || 0
        }
        
        return { success: true, data }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch groups'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchGroup(groupName: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await radiusApi.getGroup(groupName)
        this.currentGroup = response.data as RadiusGroup
        return { success: true, data: this.currentGroup }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch group'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async createGroup(groupData: CreateGroupRequest) {
      this.loading = true
      this.error = null
      
      try {
        const response = await radiusApi.createGroup(groupData)
        await this.fetchGroups() // Refresh the list
        return { success: true, data: response.data }
      } catch (error: any) {
        this.error = error.message || 'Failed to create group'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async deleteGroup(groupName: string) {
      this.loading = true
      this.error = null
      
      try {
        await radiusApi.deleteGroup(groupName)
        await this.fetchGroups() // Refresh the list
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to delete group'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async addUserToGroup(groupName: string, username: string, priority?: number) {
      this.loading = true
      this.error = null
      
      try {
        const request: AddUserToGroupRequest = { priority }
        await radiusApi.addUserToGroup(groupName, username, request)
        await this.fetchGroupUsers(groupName) // Refresh group users
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to add user to group'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async removeUserFromGroup(groupName: string, username: string) {
      this.loading = true
      this.error = null
      
      try {
        await radiusApi.removeUserFromGroup(groupName, username)
        await this.fetchGroupUsers(groupName) // Refresh group users
        return { success: true }
      } catch (error: any) {
        this.error = error.message || 'Failed to remove user from group'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchGroupUsers(groupName: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await radiusApi.getGroupUsers(groupName)
        this.groupUsers = response.data || []
        return { success: true, data: this.groupUsers }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch group users'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    async fetchUserGroups(username: string) {
      this.loading = true
      this.error = null
      
      try {
        const response = await radiusApi.getUserGroups(username)
        this.userGroups = response.data || []
        return { success: true, data: this.userGroups }
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch user groups'
        return { success: false, error: this.error }
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },

    clearCurrentGroup() {
      this.currentGroup = null
      this.groupUsers = []
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
