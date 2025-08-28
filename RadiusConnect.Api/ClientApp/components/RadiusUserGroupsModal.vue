<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-full max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-medium text-gray-900">
            Manage Groups for {{ user?.username }}
          </h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Error Display -->
        <div v-if="error" class="mb-4">
          <div class="bg-red-50 border border-red-200 rounded-lg p-3">
            <div class="flex">
              <div class="flex-shrink-0">
                <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
              </div>
              <div class="ml-3">
                <p class="text-sm text-red-700">{{ error }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex justify-center items-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span class="ml-2 text-gray-600">Loading groups...</span>
        </div>

        <!-- Content -->
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Available Groups -->
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h4 class="text-md font-medium text-gray-900">Available Groups</h4>
              <span class="text-sm text-gray-500">{{ availableGroups.length }} groups</span>
            </div>
            
            <!-- Search Available Groups -->
            <div class="relative">
              <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                v-model="availableSearch"
                type="text"
                placeholder="Search available groups..."
                class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <!-- Available Groups List -->
            <div class="border border-gray-200 rounded-lg max-h-96 overflow-y-auto">
              <div v-if="filteredAvailableGroups.length === 0" class="p-4 text-center text-gray-500">
                <UsersIcon class="mx-auto h-8 w-8 text-gray-300 mb-2" />
                <p>No available groups found</p>
              </div>
              <div v-else class="divide-y divide-gray-200">
                <div
                  v-for="group in filteredAvailableGroups"
                  :key="group.groupName"
                  class="p-3 hover:bg-gray-50 flex items-center justify-between"
                >
                  <div class="flex-1">
                    <div class="font-medium text-gray-900">{{ group.groupName }}</div>
                    <div class="text-sm text-gray-500">
                      {{ group.users.length }} users
                    </div>
                  </div>
                  <button
                    @click="addUserToGroup(group)"
                    :disabled="addingToGroup === group.groupName"
                    class="ml-3 inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                  >
                    <span v-if="addingToGroup === group.groupName" class="animate-spin rounded-full h-3 w-3 border-b-2 border-white mr-1"></span>
                    Add
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- User's Groups -->
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h4 class="text-md font-medium text-gray-900">User's Groups</h4>
              <span class="text-sm text-gray-500">{{ userGroups.length }} groups</span>
            </div>
            
            <!-- Search User Groups -->
            <div class="relative">
              <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                v-model="userGroupsSearch"
                type="text"
                placeholder="Search user groups..."
                class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <!-- User Groups List -->
            <div class="border border-gray-200 rounded-lg max-h-96 overflow-y-auto">
              <div v-if="filteredUserGroups.length === 0" class="p-4 text-center text-gray-500">
                <UsersIcon class="mx-auto h-8 w-8 text-gray-300 mb-2" />
                <p>User is not in any groups</p>
              </div>
              <div v-else class="divide-y divide-gray-200">
                <div
                  v-for="group in filteredUserGroups"
                  :key="group.groupName"
                  class="p-3 hover:bg-gray-50 flex items-center justify-between"
                >
                  <div class="flex-1">
                    <div class="font-medium text-gray-900">{{ group.groupName }}</div>
                    <div class="text-sm text-gray-500">
                      {{ group.users.length }} users
                    </div>
                  </div>
                  <button
                    @click="removeUserFromGroup(group)"
                    :disabled="removingFromGroup === group.groupName"
                    class="ml-3 inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50"
                  >
                    <span v-if="removingFromGroup === group.groupName" class="animate-spin rounded-full h-3 w-3 border-b-2 border-white mr-1"></span>
                    Remove
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="mt-6 flex justify-end space-x-3">
          <button
            @click="$emit('close')"
            type="button"
            class="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRadiusGroupsStore } from '~/stores/radiusGroups'
import type { RadiusUser, RadiusGroup } from '~/types'
import {
  XMarkIcon,
  ExclamationTriangleIcon,
  MagnifyingGlassIcon,
  UsersIcon
} from '@heroicons/vue/24/outline'

// Props
interface Props {
  user: RadiusUser | null
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
  updated: []
}>()

// Stores
const radiusGroupsStore = useRadiusGroupsStore()

// Reactive data
const loading = ref(false)
const error = ref('')
const availableSearch = ref('')
const userGroupsSearch = ref('')
const addingToGroup = ref('')
const removingFromGroup = ref('')
const allGroups = ref<RadiusGroup[]>([])
const userGroups = ref<RadiusGroup[]>([])

// Computed properties
const availableGroups = computed(() => {
  if (!props.user) return []
  return allGroups.value.filter(group => 
    !props.user!.groups.includes(group.groupName)
  )
})

const filteredAvailableGroups = computed(() => {
  if (!availableSearch.value) return availableGroups.value
  return availableGroups.value.filter(group =>
    group.groupName.toLowerCase().includes(availableSearch.value.toLowerCase())
  )
})

const filteredUserGroups = computed(() => {
  if (!userGroupsSearch.value) return userGroups.value
  return userGroups.value.filter(group =>
    group.groupName.toLowerCase().includes(userGroupsSearch.value.toLowerCase())
  )
})

// Methods
const fetchData = async () => {
  if (!props.user) return
  
  loading.value = true
  error.value = ''
  
  try {
    // Fetch all groups
    await radiusGroupsStore.fetchGroups(1, 100) // Get all groups
    allGroups.value = [...radiusGroupsStore.groups]
    
    // Filter user's groups
    userGroups.value = allGroups.value.filter(group => 
      props.user!.groups.includes(group.groupName)
    )
  } catch (err: any) {
    error.value = err.message || 'Failed to load groups'
  } finally {
    loading.value = false
  }
}

const addUserToGroup = async (group: RadiusGroup) => {
  if (!props.user) return
  
  addingToGroup.value = group.groupName
  error.value = ''
  
  try {
    await radiusGroupsStore.addUserToGroup(group.groupName, props.user.username, 1)
    
    // Update local state
    userGroups.value.push(group)
    
    // Update user's groups in the user object
    if (props.user) {
      props.user.groups.push(group.groupName)
    }
    
    emit('updated')
  } catch (err: any) {
    error.value = err.message || 'Failed to add user to group'
  } finally {
    addingToGroup.value = ''
  }
}

const removeUserFromGroup = async (group: RadiusGroup) => {
  if (!props.user) return
  
  removingFromGroup.value = group.groupName
  error.value = ''
  
  try {
    await radiusGroupsStore.removeUserFromGroup(group.groupName, props.user.username)
    
    // Update local state
    userGroups.value = userGroups.value.filter(g => g.groupName !== group.groupName)
    
    // Update user's groups in the user object
    if (props.user) {
      props.user.groups = props.user.groups.filter(g => g !== group.groupName)
    }
    
    emit('updated')
  } catch (err: any) {
    error.value = err.message || 'Failed to remove user from group'
  } finally {
    removingFromGroup.value = ''
  }
}

// Lifecycle
onMounted(() => {
  fetchData()
})

// Watchers
watch(() => props.user, () => {
  if (props.user) {
    fetchData()
  }
}, { immediate: true })
</script>

<style scoped>
/* Custom scrollbar for the group lists */
.max-h-96::-webkit-scrollbar {
  width: 6px;
}

.max-h-96::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.max-h-96::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.max-h-96::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>