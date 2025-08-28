<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">RADIUS Groups</h1>
            <p class="mt-1 text-sm text-gray-600">
              Manage RADIUS groups and user assignments
            </p>
          </div>
          <UiButton
            variant="primary"
            @click="showCreateModal = true"
            :loading="groupsStore.loading"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            Create Group
          </UiButton>
        </div>
      </div>

      <!-- Error Display -->
      <div v-if="groupsStore.error" class="mb-6 px-4 sm:px-0">
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ groupsStore.error }}</p>
            </div>
            <div class="ml-auto pl-3">
              <button
                @click="groupsStore.clearError()"
                class="text-red-400 hover:text-red-600"
              >
                <XMarkIcon class="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="groupsStore.loading && !groupsStore.hasGroups" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          <div class="flex justify-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
          <p class="text-center mt-4 text-gray-600">Loading groups...</p>
        </div>
      </div>

      <!-- Groups Table -->
      <div v-else-if="groupsStore.hasGroups" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Group Name
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Users
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Attributes
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="group in groupsStore.groups" :key="group.groupName" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <div class="flex-shrink-0 h-8 w-8">
                        <div class="h-8 w-8 rounded-lg bg-blue-100 flex items-center justify-center">
                          <UserGroupIcon class="h-4 w-4 text-blue-600" />
                        </div>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">
                          {{ group.groupName }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ group.userCount || 0 }} users
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      :class="[
                        'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                        group.isActive
                          ? 'bg-green-100 text-green-800'
                          : 'bg-gray-100 text-gray-800'
                      ]"
                    >
                      {{ group.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    <div v-if="group.checkAttributes && Object.keys(group.checkAttributes).length > 0">
                      <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                        {{ Object.keys(group.checkAttributes).length }} check
                      </span>
                    </div>
                    <div v-if="group.replyAttributes && Object.keys(group.replyAttributes).length > 0" class="mt-1">
                      <span class="text-xs bg-green-100 text-green-800 px-2 py-1 rounded">
                        {{ Object.keys(group.replyAttributes).length }} reply
                      </span>
                    </div>
                    <div v-if="(!group.checkAttributes || Object.keys(group.checkAttributes).length === 0) && 
                              (!group.replyAttributes || Object.keys(group.replyAttributes).length === 0)">
                      <span class="text-xs text-gray-400">No attributes</span>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <div class="flex justify-end space-x-2">
                      <UiButton
                        variant="secondary"
                        size="sm"
                        @click="viewGroup(group)"
                      >
                        View
                      </UiButton>
                      <UiButton
                        variant="danger"
                        size="sm"
                        @click="deleteGroup(group.groupName)"
                        :loading="groupsStore.loading"
                      >
                        Delete
                      </UiButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="groupsStore.pagination.totalPages > 1" class="bg-white px-4 py-3 border-t border-gray-200 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex-1 flex justify-between sm:hidden">
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="groupsStore.pagination.page === 1"
                  @click="goToPage(groupsStore.pagination.page - 1)"
                >
                  Previous
                </UiButton>
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="groupsStore.pagination.page === groupsStore.pagination.totalPages"
                  @click="goToPage(groupsStore.pagination.page + 1)"
                >
                  Next
                </UiButton>
              </div>
              <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p class="text-sm text-gray-700">
                    Showing
                    <span class="font-medium">{{ (groupsStore.pagination.page - 1) * groupsStore.pagination.pageSize + 1 }}</span>
                    to
                    <span class="font-medium">
                      {{ Math.min(groupsStore.pagination.page * groupsStore.pagination.pageSize, groupsStore.pagination.total) }}
                    </span>
                    of
                    <span class="font-medium">{{ groupsStore.pagination.total }}</span>
                    results
                  </p>
                </div>
                <div>
                  <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="groupsStore.pagination.page === 1"
                      @click="goToPage(groupsStore.pagination.page - 1)"
                    >
                      Previous
                    </UiButton>
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="groupsStore.pagination.page === groupsStore.pagination.totalPages"
                      @click="goToPage(groupsStore.pagination.page + 1)"
                    >
                      Next
                    </UiButton>
                  </nav>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          <div class="text-center">
            <UserGroupIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">No groups</h3>
            <p class="mt-1 text-sm text-gray-500">
              Get started by creating a new RADIUS group.
            </p>
            <div class="mt-6">
              <UiButton
                variant="primary"
                @click="showCreateModal = true"
              >
                <PlusIcon class="w-4 h-4 mr-2" />
                Create Group
              </UiButton>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Create Group Modal -->
    <FormsGroupsCreateModal
      v-if="showCreateModal"
      @close="showCreateModal = false"
      @saved="handleGroupCreated"
    />

    <!-- Group Details Modal -->
    <FormsGroupsDetailModal
      v-if="showDetailModal"
      :group="selectedGroup"
      @close="showDetailModal = false"
      @updated="handleGroupUpdated"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  PlusIcon, 
  ExclamationTriangleIcon, 
  XMarkIcon,
  UserGroupIcon 
} from '@heroicons/vue/24/outline'

definePageMeta({ 
  middleware: ['auth', 'role'],
  layout: 'default',
  meta: { roles: ['Admin', 'Manager'] } 
})

const groupsStore = useGroupsStore()
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const selectedGroup = ref(null)

// Load groups on mount
onMounted(async () => {
  await groupsStore.fetchGroups()
})

const viewGroup = async (group: any) => {
  selectedGroup.value = group
  showDetailModal.value = true
}

const deleteGroup = async (groupName: string) => {
  if (confirm(`Are you sure you want to delete the group "${groupName}"?`)) {
    const result = await groupsStore.deleteGroup(groupName)
    if (result.success) {
      // Group deleted successfully
    }
  }
}

const goToPage = async (page: number) => {
  groupsStore.setPage(page)
  await groupsStore.fetchGroups()
}

const handleGroupCreated = () => {
  showCreateModal.value = false
  // Groups list will be refreshed automatically by the store
}

const handleGroupUpdated = () => {
  showDetailModal.value = false
  // Refresh the groups list
  groupsStore.fetchGroups()
}
</script>
