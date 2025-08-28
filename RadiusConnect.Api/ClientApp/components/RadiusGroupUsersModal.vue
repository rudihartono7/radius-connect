<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="text-lg font-medium text-gray-900">
              Manage Users for {{ group?.groupName }}
            </h3>
            <p class="text-sm text-gray-600">
              Add or remove users from this RADIUS group
            </p>
          </div>
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
        </div>

        <!-- Content -->
        <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Available Users -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-4">
              <h4 class="text-md font-medium text-gray-900">Available Users</h4>
              <div class="flex items-center space-x-2">
                <UiInput
                  v-model="availableUsersSearch"
                  placeholder="Search users..."
                  class="w-48"
                >
                  <template #prefix>
                    <MagnifyingGlassIcon class="h-4 w-4 text-gray-400" />
                  </template>
                </UiInput>
              </div>
            </div>

            <div v-if="filteredAvailableUsers.length === 0" class="text-sm text-gray-500 text-center py-4">
              {{ availableUsersSearch ? 'No users found' : 'No available users' }}
            </div>

            <div v-else class="space-y-2 max-h-64 overflow-y-auto">
              <div
                v-for="user in filteredAvailableUsers"
                :key="user.username"
                class="bg-white rounded border p-3 flex items-center justify-between"
              >
                <div class="flex items-center">
                  <div class="flex-shrink-0">
                    <div class="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center">
                      <UserIcon class="h-4 w-4 text-gray-600" />
                    </div>
                  </div>
                  <div class="ml-3">
                    <div class="text-sm font-medium text-gray-900">{{ user.username }}</div>
                  </div>
                </div>
                <UiButton
                  @click="addUserToGroup(user.username)"
                  variant="primary"
                  size="sm"
                  :loading="actionLoading === user.username"
                  class="flex items-center"
                >
                  <PlusIcon class="h-4 w-4 mr-1" />
                  Add
                </UiButton>
              </div>
            </div>
          </div>

          <!-- Group Users -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-4">
              <h4 class="text-md font-medium text-gray-900">Group Users</h4>
              <div class="flex items-center space-x-2">
                <UiInput
                  v-model="groupUsersSearch"
                  placeholder="Search group users..."
                  class="w-48"
                >
                  <template #prefix>
                    <MagnifyingGlassIcon class="h-4 w-4 text-gray-400" />
                  </template>
                </UiInput>
              </div>
            </div>

            <div v-if="filteredGroupUsers.length === 0" class="text-sm text-gray-500 text-center py-4">
              {{ groupUsersSearch ? 'No users found' : 'No users in this group' }}
            </div>

            <div v-else class="space-y-2 max-h-64 overflow-y-auto">
              <div
                v-for="user in filteredGroupUsers"
                :key="user.username"
                class="bg-white rounded border p-3 flex items-center justify-between"
              >
                <div class="flex items-center">
                  <div class="flex-shrink-0">
                    <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                      <UserIcon class="h-4 w-4 text-blue-600" />
                    </div>
                  </div>
                  <div class="ml-3">
                    <div class="text-sm font-medium text-gray-900">{{ user.username }}</div>
                  </div>
                </div>
                <UiButton
                  @click="removeUserFromGroup(user.username)"
                  variant="danger"
                  size="sm"
                  :loading="actionLoading === user.username"
                  class="flex items-center"
                >
                  <MinusIcon class="h-4 w-4 mr-1" />
                  Remove
                </UiButton>
              </div>
            </div>
          </div>
        </div>

        <!-- Form Actions -->
        <div class="flex justify-end space-x-3 pt-6">
          <UiButton
            type="button"
            variant="secondary"
            @click="$emit('close')"
          >
            Close
          </UiButton>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  ExclamationTriangleIcon, 
  MagnifyingGlassIcon,
  UserIcon,
  PlusIcon,
  MinusIcon
} from '@heroicons/vue/24/outline'
import type { RadiusGroup, RadiusUser } from '~/types'

interface Props {
  group: RadiusGroup | null
}

interface Emits {
  close: []
  updated: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Stores
const radiusGroupsStore = useRadiusGroupsStore()
const radiusUsersStore = useRadiusUsersStore()

// Reactive data
const loading = ref(false)
const actionLoading = ref('')
const error = ref('')
const availableUsersSearch = ref('')
const groupUsersSearch = ref('')
const availableUsers = ref<RadiusUser[]>([])
const groupUsers = ref<RadiusUser[]>([])

// Computed properties
const filteredAvailableUsers = computed(() => {
  if (!availableUsersSearch.value) return availableUsers.value
  
  const query = availableUsersSearch.value.toLowerCase()
  return availableUsers.value.filter(user => 
    user.username.toLowerCase().includes(query)
  )
})

const filteredGroupUsers = computed(() => {
  if (!groupUsersSearch.value) return groupUsers.value
  
  const query = groupUsersSearch.value.toLowerCase()
  return groupUsers.value.filter(user => 
    user.username.toLowerCase().includes(query)
  )
})

// Methods
const loadData = async () => {
  if (!props.group) return

  loading.value = true
  error.value = ''

  try {
    // Load all users and group users in parallel
    const [allUsersResult, groupUsersResult] = await Promise.all([
      radiusUsersStore.fetchUsers(),
      radiusApi.getGroupUsers(props.group.groupName)
    ])

    // Set group users
    if (groupUsersResult.success && groupUsersResult.data) {
      groupUsers.value = groupUsersResult.data as RadiusUser[]
    } else {
      groupUsers.value = []
    }

    // Set available users (exclude users already in the group)
    const groupUsernames = new Set(groupUsers.value.map(u => u.username))
    availableUsers.value = radiusUsersStore.users.filter(user => 
      !groupUsernames.has(user.username)
    )
  } catch (err) {
    console.error('Error loading data:', err)
    error.value = 'Failed to load users data'
  } finally {
    loading.value = false
  }
}

const addUserToGroup = async (username: string) => {
  if (!props.group) return

  actionLoading.value = username
  error.value = ''

  try {
    const result = await radiusGroupsStore.addUserToGroup(props.group.groupName, username)
    
    if (result.success) {
      // Move user from available to group users
      const userIndex = availableUsers.value.findIndex(u => u.username === username)
      if (userIndex !== -1) {
        const user = availableUsers.value.splice(userIndex, 1)[0]
        if (user) {
          groupUsers.value.push(user)
        }
      }
      
      emit('updated')
    } else {
      error.value = result.message || 'Failed to add user to group'
    }
  } catch (err) {
    console.error('Error adding user to group:', err)
    error.value = err instanceof Error ? err.message : 'Failed to add user to group'
  } finally {
    actionLoading.value = ''
  }
}

const removeUserFromGroup = async (username: string) => {
  if (!props.group) return

  actionLoading.value = username
  error.value = ''

  try {
    const result = await radiusGroupsStore.removeUserFromGroup(props.group.groupName, username)
    
    if (result.success) {
      // Move user from group users to available
      const userIndex = groupUsers.value.findIndex(u => u.username === username)
      if (userIndex !== -1) {
        const user = groupUsers.value.splice(userIndex, 1)[0]
        if (user) {
          availableUsers.value.push(user)
        }
      }
      
      emit('updated')
    } else {
      error.value = result.message || 'Failed to remove user from group'
    }
  } catch (err) {
    console.error('Error removing user from group:', err)
    error.value = err instanceof Error ? err.message : 'Failed to remove user from group'
  } finally {
    actionLoading.value = ''
  }
}

// Initialize when group changes
watch(
  () => props.group,
  (newGroup) => {
    if (newGroup) {
      loadData()
    } else {
      availableUsers.value = []
      groupUsers.value = []
    }
  },
  { immediate: true }
)
</script>