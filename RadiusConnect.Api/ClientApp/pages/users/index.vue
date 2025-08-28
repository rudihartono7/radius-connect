<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-semibold text-gray-900">Users</h1>
            <p class="mt-1 text-sm text-gray-600">
              Manage system users and their permissions
            </p>
          </div>
          <UiButton @click="showCreateModal = true" variant="primary">
            <PlusIcon class="w-4 h-4 mr-2" />
            Add User
          </UiButton>
        </div>
      </div>

      <!-- Search and Filters -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <UiInput
              v-model="searchQuery"
              placeholder="Search users..."
              @input="handleSearch"
            >
              <template #prefix>
                <MagnifyingGlassIcon class="w-4 h-4 text-gray-400" />
              </template>
            </UiInput>
          </div>
          <div class="flex gap-2">
            <UiButton variant="outline" @click="refreshUsers">
              <ArrowPathIcon class="w-4 h-4" />
            </UiButton>
          </div>
        </div>
      </div>

      <!-- Users Table -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  User
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Roles
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Last Login
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="user in usersStore.users" :key="user.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <div class="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
                      <span class="text-gray-700 font-medium text-sm">
                        {{ getUserInitials(user) }}
                      </span>
                    </div>
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900">
                        {{ user.firstName }} {{ user.lastName }}
                      </div>
                      <div class="text-sm text-gray-500">
                        {{ user.username }}
                      </div>
                      <div class="text-sm text-gray-500">
                        {{ user.email }}
                      </div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span
                    :class="[
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      user.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-red-100 text-red-800'
                    ]"
                  >
                    {{ user.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex flex-wrap gap-1">
                    <span
                      v-for="role in user.roles"
                      :key="role"
                      class="inline-flex px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full"
                    >
                      {{ role }}
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatDate(user.lastLogin) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <div class="flex items-center space-x-2">
                    <UiButton
                      size="sm"
                      variant="outline"
                      @click="editUser(user)"
                    >
                      Edit
                    </UiButton>
                    <UiButton
                      v-if="user.isActive"
                      size="sm"
                      variant="secondary"
                      @click="deactivateUser(user.id)"
                    >
                      Deactivate
                    </UiButton>
                    <UiButton
                      v-else
                      size="sm"
                      variant="primary"
                      @click="activateUser(user.id)"
                    >
                      Activate
                    </UiButton>
                    <UiButton
                      size="sm"
                      variant="danger"
                      @click="deleteUser(user.id)"
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
        <div class="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
          <div class="flex-1 flex justify-between sm:hidden">
            <UiButton
              :disabled="usersStore.currentPage === 1"
              variant="outline"
              size="sm"
              @click="goToPage(usersStore.currentPage - 1)"
            >
              Previous
            </UiButton>
            <UiButton
              :disabled="usersStore.currentPage === usersStore.totalPages"
              variant="outline"
              size="sm"
              @click="goToPage(usersStore.currentPage + 1)"
            >
              Next
            </UiButton>
          </div>
          <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
              <p class="text-sm text-gray-700">
                Showing
                <span class="font-medium">{{ (usersStore.currentPage - 1) * usersStore.pageSize + 1 }}</span>
                to
                <span class="font-medium">
                  {{ Math.min(usersStore.currentPage * usersStore.pageSize, usersStore.totalCount) }}
                </span>
                of
                <span class="font-medium">{{ usersStore.totalCount }}</span>
                results
              </p>
            </div>
            <div>
              <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                <UiButton
                  :disabled="usersStore.currentPage === 1"
                  variant="outline"
                  size="sm"
                  @click="goToPage(usersStore.currentPage - 1)"
                >
                  Previous
                </UiButton>
                <UiButton
                  :disabled="usersStore.currentPage === usersStore.totalPages"
                  variant="outline"
                  size="sm"
                  @click="goToPage(usersStore.currentPage + 1)"
                >
                  Next
                </UiButton>
              </nav>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Create User Modal -->
    <FormsUserFormModal
      v-if="showCreateModal"
      :user="null"
      @close="showCreateModal = false"
      @saved="handleUserSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  PlusIcon, 
  MagnifyingGlassIcon, 
  ArrowPathIcon 
} from '@heroicons/vue/24/outline'

definePageMeta({
  middleware: ['auth', 'role'],
  layout: 'default',
  meta: {
    roles: ['Admin', 'Manager']
  }
})

const usersStore = useUsersStore()
const searchQuery = ref('')
const showCreateModal = ref(false)

// Load users on page mount
onMounted(async () => {
  await usersStore.fetchUsers()

  console.log(usersStore.currentPage, usersStore.pageSize);
})

const handleSearch = useDebounceFn(async () => {
  await usersStore.fetchUsers({
    search: searchQuery.value,
    page: 1
  })
}, 300)

const refreshUsers = async () => {
  await usersStore.fetchUsers({
    search: searchQuery.value,
    page: usersStore.currentPage
  })
}

const goToPage = async (page: number) => {
  await usersStore.fetchUsers({
    search: searchQuery.value,
    page
  })
}

const getUserInitials = (user: any) => {
  const firstName = user.firstName || ''
  const lastName = user.lastName || ''
  
  if (firstName && lastName) {
    return `${firstName[0]}${lastName[0]}`.toUpperCase()
  }
  
  return user.username.substring(0, 2).toUpperCase()
}

const formatDate = (date?: string) => {
  if (!date) return 'Never'
  return new Date(date).toLocaleDateString()
}

const editUser = (user: any) => {
  // TODO: Implement edit modal
  console.log('Edit user:', user)
}

const activateUser = async (userId: string) => {
  const result = await usersStore.activateUser(userId)
  if (!result.success) {
    // TODO: Show error notification
    console.error('Failed to activate user:', result.error)
  }
}

const deactivateUser = async (userId: string) => {
  const result = await usersStore.deactivateUser(userId)
  if (!result.success) {
    // TODO: Show error notification
    console.error('Failed to deactivate user:', result.error)
  }
}

const deleteUser = async (userId: string) => {
  if (confirm('Are you sure you want to delete this user?')) {
    const result = await usersStore.deleteUser(userId)
    if (!result.success) {
      // TODO: Show error notification
      console.error('Failed to delete user:', result.error)
    }
  }
}

const handleUserSaved = () => {
  showCreateModal.value = false
  refreshUsers()
}
</script>
