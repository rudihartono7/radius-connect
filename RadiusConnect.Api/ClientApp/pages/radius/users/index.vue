<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">RADIUS Users</h1>
            <p class="mt-1 text-sm text-gray-600">
              Manage RADIUS authentication users and their attributes
            </p>
          </div>
          <UiButton
            variant="primary"
            @click="showCreateModal = true"
            :loading="radiusUsersStore.loading"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            Add RADIUS User
          </UiButton>
        </div>
      </div>

      <!-- Error Display -->
      <div v-if="radiusUsersStore.error" class="mb-6 px-4 sm:px-0">
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ radiusUsersStore.error }}</p>
            </div>
            <div class="ml-auto pl-3">
              <button
                @click="radiusUsersStore.clearError()"
                class="text-red-400 hover:text-red-600"
              >
                <XMarkIcon class="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Search and Filters -->
      <div class="bg-white shadow rounded-lg mb-6">
        <div class="px-4 py-5 sm:p-6">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <UiInput
                v-model="searchQuery"
                placeholder="Search RADIUS users..."
                type="text"
                @input="debouncedSearch"
              >
                <template #prefix>
                  <MagnifyingGlassIcon class="h-5 w-5 text-gray-400" />
                </template>
              </UiInput>
            </div>
            <div class="flex space-x-2">
              <UiButton
                variant="secondary"
                @click="refreshUsers"
                :loading="radiusUsersStore.loading"
              >
                <ArrowPathIcon class="w-4 h-4 mr-2" />
                Refresh
              </UiButton>
            </div>
          </div>
        </div>
      </div>

      <!-- Users Table -->
      <div class="bg-white shadow overflow-hidden sm:rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <!-- Loading State -->
          <div v-if="radiusUsersStore.loading" class="flex justify-center items-center py-12">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>

          <!-- Empty State -->
          <div v-else-if="radiusUsersStore.users.length === 0" class="text-center py-12">
            <UserIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">No RADIUS users</h3>
            <p class="mt-1 text-sm text-gray-500">Get started by creating a new RADIUS user.</p>
            <div class="mt-6">
              <UiButton variant="primary" @click="showCreateModal = true">
                <PlusIcon class="w-4 h-4 mr-2" />
                Add RADIUS User
              </UiButton>
            </div>
          </div>

          <!-- Users Table -->
          <div v-else class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Username
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Groups
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Attributes
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Last Auth
                  </th>
                  <th class="relative px-6 py-3">
                    <span class="sr-only">Actions</span>
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="user in radiusUsersStore.users" :key="user.username" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <div class="flex-shrink-0 h-8 w-8">
                        <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                          <span class="text-sm font-medium text-blue-600">{{ getUserInitials(user.username) }}</span>
                        </div>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">{{ user.username }}</div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span :class="[
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                    ]">
                      {{ user.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex flex-wrap gap-1">
                      <span
                        v-for="group in user.groups?.slice(0, 2)"
                        :key="group"
                        class="inline-flex px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full"
                      >
                        {{ group }}
                      </span>
                      <span
                        v-if="user.groups && user.groups.length > 2"
                        class="inline-flex px-2 py-1 text-xs font-medium bg-gray-100 text-gray-600 rounded-full"
                      >
                        +{{ user.groups.length - 2 }}
                      </span>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ (user.checkAttributes?.length || 0) + (user.replyAttributes?.length || 0) }} attributes
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ user.lastAuth ? formatDate(user.lastAuth) : 'Never' }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <div class="flex justify-end space-x-2">
                      <button
                        @click="editUser(user)"
                        class="text-blue-600 hover:text-blue-900"
                        title="Edit user"
                      >
                        <PencilIcon class="h-4 w-4" />
                      </button>
                      <button
                        @click="manageAttributes(user)"
                        class="text-green-600 hover:text-green-900"
                        title="Manage attributes"
                      >
                        <Cog6ToothIcon class="h-4 w-4" />
                      </button>
                      <button
                        @click="deleteUser(user)"
                        class="text-red-600 hover:text-red-900"
                        title="Delete user"
                      >
                        <TrashIcon class="h-4 w-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="radiusUsersStore.users.length > 0" class="mt-6 flex items-center justify-between">
            <div class="text-sm text-gray-700">
              Showing {{ ((radiusUsersStore.currentPage - 1) * radiusUsersStore.pageSize) + 1 }} to 
              {{ Math.min(radiusUsersStore.currentPage * radiusUsersStore.pageSize, radiusUsersStore.totalCount) }} of 
              {{ radiusUsersStore.totalCount }} results
            </div>
            <div class="flex space-x-2">
              <UiButton
                variant="secondary"
                size="sm"
                :disabled="radiusUsersStore.currentPage <= 1"
                @click="goToPage(radiusUsersStore.currentPage - 1)"
              >
                Previous
              </UiButton>
              <UiButton
                variant="secondary"
                size="sm"
                :disabled="radiusUsersStore.currentPage >= radiusUsersStore.totalPages"
                @click="goToPage(radiusUsersStore.currentPage + 1)"
              >
                Next
              </UiButton>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Create/Edit Modal -->
    <RadiusUserFormModal
      v-if="showCreateModal"
      :user="selectedUser"
      @close="showCreateModal = false; selectedUser = null"
      @saved="handleUserSaved"
    />

    <!-- Attributes Modal -->
    <RadiusAttributesModal
      v-if="showAttributesModal"
      :user="selectedUser"
      @close="showAttributesModal = false; selectedUser = null"
      @saved="handleAttributesSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  PlusIcon, 
  MagnifyingGlassIcon, 
  ArrowPathIcon, 
  ExclamationTriangleIcon,
  XMarkIcon,
  UserIcon,
  PencilIcon,
  TrashIcon,
  Cog6ToothIcon
} from '@heroicons/vue/24/outline'
import type { RadiusUser } from '~/types'

// Page meta
definePageMeta({
  middleware: 'auth',
  requiresAuth: true,
  roles: ['Admin', 'Manager']
})

// Store
const radiusUsersStore = useRadiusUsersStore()

// Reactive data
const searchQuery = ref('')
const showCreateModal = ref(false)
const showAttributesModal = ref(false)
const selectedUser = ref<RadiusUser | null>(null)

// Debounced search
const debouncedSearch = useDebounceFn(() => {
  radiusUsersStore.fetchUsers(1, radiusUsersStore.pageSize, searchQuery.value)
}, 300)

// Methods
const refreshUsers = async () => {
  await radiusUsersStore.fetchUsers(
    radiusUsersStore.currentPage, 
    radiusUsersStore.pageSize, 
    searchQuery.value
  )
}

const goToPage = (page: number) => {
  radiusUsersStore.fetchUsers(page, radiusUsersStore.pageSize, searchQuery.value ?? "")
}

const getUserInitials = (username: string) => {
  return username.substring(0, 2).toUpperCase()
}

const formatDate = (date: string | Date) => {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const editUser = (user: RadiusUser) => {
  selectedUser.value = user
  showCreateModal.value = true
}

const manageAttributes = (user: RadiusUser) => {
  selectedUser.value = user
  showAttributesModal.value = true
}

const deleteUser = async (user: RadiusUser) => {
  if (confirm(`Are you sure you want to delete RADIUS user "${user.username}"? This action cannot be undone.`)) {
    const result = await radiusUsersStore.deleteUser(user.username)
    if (result.success) {
      await refreshUsers()
    }
  }
}

const handleUserSaved = async () => {
  showCreateModal.value = false
  selectedUser.value = null
  await refreshUsers()
}

const handleAttributesSaved = async () => {
  showAttributesModal.value = false
  selectedUser.value = null
  await refreshUsers()
}

// Lifecycle
onMounted(() => {
  radiusUsersStore.fetchUsers()
})
</script>