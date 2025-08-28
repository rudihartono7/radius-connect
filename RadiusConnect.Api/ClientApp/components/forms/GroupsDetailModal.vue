<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <h3 class="text-xl font-medium text-gray-900">
              Group: {{ group?.groupName }}
            </h3>
            <p class="text-sm text-gray-600 mt-1">
              Manage group details and user assignments
            </p>
          </div>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <!-- Content -->
        <div v-else class="space-y-6">
          <!-- Group Information -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Group Information</h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Group Name</label>
                <p class="mt-1 text-sm text-gray-900">{{ group?.groupName }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Status</label>
                <span
                  :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full mt-1',
                    group?.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  ]"
                >
                  {{ group?.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Attributes -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Attributes</h4>
            
            <!-- Check Attributes -->
            <div v-if="group?.checkAttributes && Object.keys(group.checkAttributes).length > 0" class="mb-4">
              <h5 class="text-sm font-medium text-gray-700 mb-2">Check Attributes</h5>
              <div class="bg-white rounded border p-3">
                <div v-for="(value, key) in group.checkAttributes" :key="key" class="flex justify-between py-1">
                  <span class="text-sm font-medium text-gray-900">{{ key }}</span>
                  <span class="text-sm text-gray-600">{{ value }}</span>
                </div>
              </div>
            </div>

            <!-- Reply Attributes -->
            <div v-if="group?.replyAttributes && Object.keys(group.replyAttributes).length > 0" class="mb-4">
              <h5 class="text-sm font-medium text-gray-700 mb-2">Reply Attributes</h5>
              <div class="bg-white rounded border p-3">
                <div v-for="(value, key) in group.replyAttributes" :key="key" class="flex justify-between py-1">
                  <span class="text-sm font-medium text-gray-900">{{ key }}</span>
                  <span class="text-sm text-gray-600">{{ value }}</span>
                </div>
              </div>
            </div>

            <div v-if="(!group?.checkAttributes || Object.keys(group.checkAttributes || {}).length === 0) && 
                      (!group?.replyAttributes || Object.keys(group.replyAttributes || {}).length === 0)">
              <p class="text-sm text-gray-500">No attributes configured</p>
            </div>
          </div>

          <!-- Group Users -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-4">
              <h4 class="text-lg font-medium text-gray-900">Group Users</h4>
              <UiButton
                variant="primary"
                size="sm"
                @click="showAddUserModal = true"
              >
                <PlusIcon class="w-4 h-4 mr-2" />
                Add User
              </UiButton>
            </div>

            <!-- Users List -->
            <div v-if="groupUsers.length > 0" class="bg-white rounded border overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Username
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Priority
                    </th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr v-for="user in groupUsers" :key="user.username" class="hover:bg-gray-50">
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <div class="flex-shrink-0 h-8 w-8">
                          <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                            <span class="text-sm font-medium text-blue-600">
                              {{ user.username.charAt(0).toUpperCase() }}
                            </span>
                          </div>
                        </div>
                        <div class="ml-4">
                          <div class="text-sm font-medium text-gray-900">
                            {{ user.username }}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {{ user.priority || 'Default' }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <UiButton
                        variant="danger"
                        size="sm"
                        @click="removeUserFromGroup(user.username)"
                        :loading="loading"
                      >
                        Remove
                      </UiButton>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Empty State -->
            <div v-else class="bg-white rounded border p-8 text-center">
              <UserIcon class="mx-auto h-12 w-12 text-gray-400" />
              <h3 class="mt-2 text-sm font-medium text-gray-900">No users in group</h3>
              <p class="mt-1 text-sm text-gray-500">
                Add users to this group to get started.
              </p>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
          <UiButton
            variant="secondary"
            @click="$emit('close')"
          >
            Close
          </UiButton>
        </div>
      </div>
    </div>

    <!-- Add User Modal -->
    <GroupsAddUserModal
      v-if="showAddUserModal"
      :group-name="group?.groupName"
      @close="showAddUserModal = false"
      @user-added="handleUserAdded"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  PlusIcon, 
  UserIcon 
} from '@heroicons/vue/24/outline'

interface Props {
  group: any
}

const props = defineProps<Props>()
const emit = defineEmits<{ close: []; updated: [] }>()

const groupsStore = useGroupsStore()
const showAddUserModal = ref(false)
const loading = ref(false)
const groupUsers = ref<any[]>([])

// Load group users when modal opens
onMounted(async () => {
  if (props.group?.groupName) {
    await loadGroupUsers()
  }
})

const loadGroupUsers = async () => {
  loading.value = true
  try {
    const result = await groupsStore.fetchGroupUsers(props.group.groupName)
    if (result.success) {
      groupUsers.value = result.data || []
    }
  } catch (error) {
    console.error('Failed to load group users:', error)
  } finally {
    loading.value = false
  }
}

const removeUserFromGroup = async (username: string) => {
  if (confirm(`Are you sure you want to remove ${username} from this group?`)) {
    loading.value = true
    try {
      const result = await groupsStore.removeUserFromGroup(props.group.groupName, username)
      if (result.success) {
        await loadGroupUsers() // Refresh the list
      }
    } catch (error) {
      console.error('Failed to remove user from group:', error)
    } finally {
      loading.value = false
    }
  }
}

const handleUserAdded = async () => {
  showAddUserModal.value = false
  await loadGroupUsers() // Refresh the list
  emit('updated')
}
</script>
