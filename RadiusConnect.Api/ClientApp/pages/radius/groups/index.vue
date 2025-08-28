<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div class="container mx-auto px-4 py-6">
    <!-- Page Header -->
    <div class="mb-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">RADIUS Groups</h1>
          <p class="text-gray-600 mt-1">Manage RADIUS user groups and their assignments</p>
        </div>
        <UiButton
          v-if="canManageGroups"
          @click="openCreateModal"
          variant="primary"
          class="flex items-center"
        >
          <PlusIcon class="h-5 w-5 mr-2" />
          Add Group
        </UiButton>
      </div>
    </div>

    <!-- Search and Filters -->
    <div class="bg-white rounded-lg shadow-sm border p-4 mb-6">
      <div class="flex flex-col sm:flex-row gap-4">
        <div class="flex-1">
          <UiInput
            v-model="searchQuery"
            placeholder="Search groups by name..."
            class="w-full"
          >
            <template #prefix>
              <MagnifyingGlassIcon class="h-5 w-5 text-gray-400" />
            </template>
          </UiInput>
        </div>
        <div class="flex items-center space-x-2">
          <UiButton
            @click="refreshGroups"
            variant="secondary"
            :loading="loading"
            class="flex items-center"
          >
            <ArrowPathIcon class="h-4 w-4 mr-2" />
            Refresh
          </UiButton>
        </div>
      </div>
    </div>

    <!-- Error Display -->
    <div v-if="error" class="mb-6">
      <div class="bg-red-50 border border-red-200 rounded-lg p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-800">Error</h3>
            <p class="text-sm text-red-700 mt-1">{{ error }}</p>
          </div>
          <div class="ml-auto pl-3">
            <button
              @click="clearError"
              class="text-red-400 hover:text-red-600"
            >
              <XMarkIcon class="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Groups Table -->
    <div class="bg-white rounded-lg shadow-sm border overflow-hidden">
      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredGroups.length === 0" class="text-center py-12">
        <UserGroupIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">
          {{ searchQuery ? 'No groups found' : 'No groups created yet' }}
        </h3>
        <p class="mt-1 text-sm text-gray-500">
          {{ searchQuery ? 'Try adjusting your search criteria.' : 'Get started by creating your first RADIUS group.' }}
        </p>
        <div v-if="!searchQuery && canManageGroups" class="mt-6">
          <UiButton @click="openCreateModal" variant="primary">
            <PlusIcon class="h-5 w-5 mr-2" />
            Add Group
          </UiButton>
        </div>
      </div>

      <!-- Groups Table -->
      <div v-else>
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Group Name
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Description
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Users Count
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Created
              </th>
              <th v-if="canManageGroups" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="group in paginatedGroups" :key="group.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0">
                    <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                      <UserGroupIcon class="h-4 w-4 text-blue-600" />
                    </div>
                  </div>
                  <div class="ml-3">
                    <div class="text-sm font-medium text-gray-900">{{ group.groupName }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="text-sm text-gray-900">No description</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {{ group.userCount || 0 }} users
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                N/A
              </td>
              <td v-if="canManageGroups" class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <div class="flex items-center justify-end space-x-2">
                  <UiButton
                    @click="openManageUsersModal(group)"
                    variant="secondary"
                    size="sm"
                    class="flex items-center"
                  >
                    <UsersIcon class="h-4 w-4 mr-1" />
                    Users
                  </UiButton>
                  <UiButton
                    @click="openEditModal(group)"
                    variant="secondary"
                    size="sm"
                    class="flex items-center"
                  >
                    <PencilIcon class="h-4 w-4 mr-1" />
                    Edit
                  </UiButton>
                  <UiButton
                    @click="confirmDelete(group)"
                    variant="danger"
                    size="sm"
                    class="flex items-center"
                  >
                    <TrashIcon class="h-4 w-4 mr-1" />
                    Delete
                  </UiButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="bg-white px-4 py-3 border-t border-gray-200 sm:px-6">
          <div class="flex items-center justify-between">
            <div class="flex-1 flex justify-between sm:hidden">
              <UiButton
                @click="previousPage"
                :disabled="currentPage === 1"
                variant="secondary"
              >
                Previous
              </UiButton>
              <UiButton
                @click="nextPage"
                :disabled="currentPage === totalPages"
                variant="secondary"
              >
                Next
              </UiButton>
            </div>
            <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p class="text-sm text-gray-700">
                  Showing
                  <span class="font-medium">{{ (currentPage - 1) * pageSize + 1 }}</span>
                  to
                  <span class="font-medium">{{ Math.min(currentPage * pageSize, filteredGroups.length) }}</span>
                  of
                  <span class="font-medium">{{ filteredGroups.length }}</span>
                  results
                </p>
              </div>
              <div>
                <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                  <button
                    @click="previousPage"
                    :disabled="currentPage === 1"
                    class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <ChevronLeftIcon class="h-5 w-5" />
                  </button>
                  <button
                    v-for="page in visiblePages"
                    :key="page"
                    @click="goToPage(page)"
                    :class="[
                      'relative inline-flex items-center px-4 py-2 border text-sm font-medium',
                      page === currentPage
                        ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                        : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                    ]"
                  >
                    {{ page }}
                  </button>
                  <button
                    @click="nextPage"
                    :disabled="currentPage === totalPages"
                    class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <ChevronRightIcon class="h-5 w-5" />
                  </button>
                </nav>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <RadiusGroupFormModal
      v-if="showFormModal"
      :group="selectedGroup"
      @close="closeFormModal"
      @saved="handleGroupSaved"
    />

    <RadiusGroupUsersModal
      v-if="showUsersModal"
      :group="selectedGroup"
      @close="closeUsersModal"
      @updated="handleGroupUpdated"
    />

    <!-- Delete Confirmation Modal -->
    <UiConfirmModal
      v-if="showDeleteModal"
      title="Delete Group"
      :message="`Are you sure you want to delete the group '${selectedGroup?.groupName}'? This action cannot be undone.`"
      confirm-text="Delete"
      confirm-variant="danger"
      @confirm="handleDelete"
      @cancel="closeDeleteModal"
    />
  </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import {
  PlusIcon,
  MagnifyingGlassIcon,
  ArrowPathIcon,
  ExclamationTriangleIcon,
  XMarkIcon,
  UserGroupIcon,
  UsersIcon,
  PencilIcon,
  TrashIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import type { RadiusGroup } from '~/types'

// Page metadata
definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

// Stores
const authStore = useAuthStore()
const radiusGroupsStore = useRadiusGroupsStore()

// Reactive data
const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const showFormModal = ref(false)
const showUsersModal = ref(false)
const showDeleteModal = ref(false)
const selectedGroup = ref<RadiusGroup | null>(null)

// Computed properties
const canManageGroups = computed(() => {
  const userRoles = authStore.user?.roles || []
  return userRoles.includes('Admin') || userRoles.includes('Manager')
})

const loading = computed(() => radiusGroupsStore.loading)
const error = computed(() => radiusGroupsStore.error)
const groups = computed(() => radiusGroupsStore.groups)

const filteredGroups = computed(() => {
  if (!searchQuery.value) return groups.value
  
  const query = searchQuery.value.toLowerCase()
  return groups.value.filter(group => 
    group.groupName.toLowerCase().includes(query)
  )
})

const totalPages = computed(() => Math.ceil(filteredGroups.value.length / pageSize.value))

const paginatedGroups = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredGroups.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 4) {
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
      pages.push('...', total)
    } else if (current >= total - 3) {
      pages.push(1, '...')
      for (let i = total - 4; i <= total; i++) {
        pages.push(i)
      }
    } else {
      pages.push(1, '...', current - 1, current, current + 1, '...', total)
    }
  }
  
  return pages
})

// Methods
const refreshGroups = async () => {
  await radiusGroupsStore.fetchGroups()
}

const clearError = () => {
  radiusGroupsStore.clearError()
}

const openCreateModal = () => {
  selectedGroup.value = null
  showFormModal.value = true
}

const openEditModal = (group: RadiusGroup) => {
  selectedGroup.value = group
  showFormModal.value = true
}

const closeFormModal = () => {
  showFormModal.value = false
  selectedGroup.value = null
}

const openManageUsersModal = (group: RadiusGroup) => {
  selectedGroup.value = group
  showUsersModal.value = true
}

const closeUsersModal = () => {
  showUsersModal.value = false
  selectedGroup.value = null
}

const confirmDelete = (group: RadiusGroup) => {
  selectedGroup.value = group
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  selectedGroup.value = null
}

const handleDelete = async () => {
  if (!selectedGroup.value) return
  
  const result = await radiusGroupsStore.deleteGroup(selectedGroup.value.groupName)
  if (result.success) {
    closeDeleteModal()
    await refreshGroups()
  }
}

const handleGroupSaved = async () => {
  closeFormModal()
  await refreshGroups()
}

const handleGroupUpdated = async () => {
  closeUsersModal()
  await refreshGroups()
}

const previousPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

const goToPage = (page: number | string) => {
  if (typeof page === 'number' && page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}

const formatDate = (dateString: string | undefined) => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Watch for search query changes to reset pagination
watch(searchQuery, () => {
  currentPage.value = 1
})

// Initialize data
onMounted(async () => {
  if (!canManageGroups.value) {
    await navigateTo('/dashboard')
    return
  }
  
  await refreshGroups()
})
</script>