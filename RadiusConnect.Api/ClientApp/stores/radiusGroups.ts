import { defineStore } from 'pinia'
import type { RadiusGroup, CreateRadiusGroupRequest, UpdateRadiusGroupRequest, RadiusAttribute, RadiusAttributeDto, AddUserToGroupRequest, ApiResponse, PagedApiResponse } from '~/types'
import { radiusApi } from '~/utils/api'

export const useRadiusGroupsStore = defineStore('radiusGroups', () => {
  // State
  const groups = ref<RadiusGroup[]>([])
  const loading = ref(false)
  const error = ref('')
  const currentPage = ref(1)
  const pageSize = ref(10)
  const totalCount = ref(0)
  const totalPages = ref(0)

  // Actions
  const clearError = () => {
    error.value = ''
  }

  const setError = (message: string) => {
    error.value = message
  }

  const fetchGroups = async (page: number = 1, size: number = 10) => {
    loading.value = true
    clearError()

    try {
      const result = await radiusApi.getGroups({ page, pageSize: size }) as PagedApiResponse<RadiusGroup[]>
      
      if (result.success && result.data) {
        groups.value = result.data || []
        currentPage.value = page
        pageSize.value = size
        totalCount.value = result.totalCount || 0
        totalPages.value = Math.ceil((result.totalCount || 0) / size)
      } else {
        setError(result.message || 'Failed to fetch groups')
      }
    } catch (err) {
      console.error('Error fetching groups:', err)
      setError(err instanceof Error ? err.message : 'Failed to fetch groups')
    } finally {
      loading.value = false
    }
  }

  const fetchGroup = async (groupName: string): Promise<ApiResponse<RadiusGroup | null>> => {
    loading.value = true
    clearError()

    try {
      const result = await radiusApi.getGroup(groupName)
      
      if (result.success && result.data) {
        // Update the group in the local state if it exists
        const groupIndex = groups.value.findIndex(g => g.groupName === groupName)
        if (groupIndex !== -1) {
          groups.value[groupIndex] = result.data as RadiusGroup
        }
      }
      
      return result as ApiResponse<RadiusGroup | null>
    } catch (err) {
      console.error('Error fetching group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: null
      }
    } finally {
      loading.value = false
    }
  }

  const createGroup = async (groupData: CreateRadiusGroupRequest): Promise<ApiResponse<RadiusGroup | null>> => {
    loading.value = true
    clearError()

    try {
      // Ensure groupData has the required structure for backend
      const createRequest: CreateRadiusGroupRequest = {
        groupName: groupData.groupName,
        checkAttributes: groupData.checkAttributes || [],
        replyAttributes: groupData.replyAttributes || []
      }

      const result = await radiusApi.createGroup(createRequest)
      
      if (result.success && result.data) {
        // Add the new group to the local state
        groups.value.unshift(result.data as RadiusGroup)
        totalCount.value += 1
      }
      
      return result as ApiResponse<RadiusGroup | null>
    } catch (err) {
      console.error('Error creating group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to create group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: null
      }
    } finally {
      loading.value = false
    }
  }

  const updateGroup = async (groupName: string, groupData: UpdateRadiusGroupRequest): Promise<ApiResponse<RadiusGroup | null>> => {
    loading.value = true
    clearError()

    try {
      // Ensure groupData has the required structure for backend
      const updateRequest: UpdateRadiusGroupRequest = {
        checkAttributes: groupData.checkAttributes || [],
        replyAttributes: groupData.replyAttributes || []
      }

      const result = await radiusApi.updateGroup(groupName, updateRequest)
      
      if (result.success && result.data) {
        // Update the group in the local state
        const groupIndex = groups.value.findIndex(g => g.groupName === groupName)
        if (groupIndex !== -1 && groups.value[groupIndex]) {
          groups.value[groupIndex] = result.data as RadiusGroup
        }
      }
      
      return result as ApiResponse<RadiusGroup | null>
    } catch (err) {
      console.error('Error updating group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to update group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: null
      }
    } finally {
      loading.value = false
    }
  }

  const deleteGroup = async (groupName: string): Promise<ApiResponse<void>> => {
    loading.value = true
    clearError()

    try {
      const result = await radiusApi.deleteGroup(groupName)
      
      if (result.success) {
        // Remove the group from the local state
        const groupIndex = groups.value.findIndex(g => g.groupName === groupName)
        if (groupIndex !== -1) {
          groups.value.splice(groupIndex, 1)
          totalCount.value -= 1
        }
      }
      
      return result as ApiResponse<void>
    } catch (err) {
      console.error('Error deleting group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: undefined
      }
    } finally {
      loading.value = false
    }
  }

  const addUserToGroup = async (groupName: string, username: string, priority: number = 1): Promise<ApiResponse<void>> => {
    loading.value = true
    clearError()

    try {
      const request: AddUserToGroupRequest = { priority }
      const result = await radiusApi.addUserToGroup(groupName, username, request)
      if (result.success) {
        // Update the group's users list
        const groupIndex = groups.value.findIndex(g => g.groupName === groupName)
        if (groupIndex !== -1 && groups.value[groupIndex]) {
          if (!groups.value[groupIndex].users.includes(username)) {
            groups.value[groupIndex].users.push(username)
          }
        }
      }
      
      return result as ApiResponse<void>
    } catch (err) {
      console.error('Error adding user to group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to add user to group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: undefined
      }
    } finally {
      loading.value = false
    }
  }

  const removeUserFromGroup = async (groupName: string, username: string): Promise<ApiResponse<void>> => {
    loading.value = true
    clearError()

    try {
      const result = await radiusApi.removeUserFromGroup(groupName, username)
      
      if (result.success) {
        // Update the group's users list
        const groupIndex = groups.value.findIndex(g => g.groupName === groupName)
        if (groupIndex !== -1 && groups.value[groupIndex]) {
          const userIndex = groups.value[groupIndex].users.indexOf(username)
          if (userIndex !== -1) {
            groups.value[groupIndex].users.splice(userIndex, 1)
          }
        }
      }
      
      return result as ApiResponse<void>
    } catch (err) {
      console.error('Error removing user from group:', err)
      const errorMessage = err instanceof Error ? err.message : 'Failed to remove user from group'
      setError(errorMessage)
      return {
        success: false,
        message: errorMessage,
        data: undefined
      }
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    groups: readonly(groups),
    loading: readonly(loading),
    error: readonly(error),
    currentPage: readonly(currentPage),
    pageSize: readonly(pageSize),
    totalCount: readonly(totalCount),
    totalPages: readonly(totalPages),
    
    // Actions
    clearError,
    setError,
    fetchGroups,
    fetchGroup,
    createGroup,
    updateGroup,
    deleteGroup,
    addUserToGroup,
    removeUserFromGroup
  }
})