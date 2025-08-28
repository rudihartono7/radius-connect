<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">Active Sessions</h1>
            <p class="mt-1 text-sm text-gray-600">
              Monitor and manage active RADIUS user sessions
            </p>
          </div>
          <div class="flex space-x-3">
            <UiButton
              variant="secondary"
              @click="refreshSessions"
              :loading="sessionsStore.loading"
            >
              <ArrowPathIcon class="w-4 h-4 mr-2" />
              Refresh
            </UiButton>
            <UiButton
              variant="primary"
              @click="showSessionStats = true"
            >
              <ChartBarIcon class="w-4 h-4 mr-2" />
              View Stats
            </UiButton>
          </div>
        </div>
      </div>

      <!-- Error Display -->
      <div v-if="sessionsStore.error" class="mb-6 px-4 sm:px-0">
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ sessionsStore.error }}</p>
            </div>
            <div class="ml-auto pl-3">
              <button
                @click="sessionsStore.clearError()"
                class="text-red-400 hover:text-red-600"
              >
                <XMarkIcon class="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="sessionsStore.loading && !sessionsStore.hasSessions" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          <div class="flex justify-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
          <p class="text-center mt-4 text-gray-600">Loading sessions...</p>
        </div>
      </div>

      <!-- Sessions Table -->
      <div v-else-if="sessionsStore.hasSessions" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    User
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Session ID
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    NAS IP
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Duration
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Data Usage
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="session in sessionsStore.sessions" :key="session.sessionId" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <div class="flex-shrink-0 h-8 w-8">
                        <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                          <span class="text-sm font-medium text-blue-600">
                            {{ session.username.charAt(0).toUpperCase() }}
                          </span>
                        </div>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">
                          {{ session.username }}
                        </div>
                        <div class="text-sm text-gray-500">
                          {{ session.framedIpAddress || 'No IP' }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900 font-mono">
                      {{ session.sessionId.substring(0, 8) }}...
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ formatDate(session.startTime) }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ session.nasIpAddress }}
                    </div>
                    <div class="text-sm text-gray-500">
                      Port {{ session.nasPort }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ formatDuration(session.sessionTime) }}
                    </div>
                    <div class="text-sm text-gray-500">
                      Started {{ formatRelativeTime(session.startTime) }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      ↓ {{ formatBytes(session.inputOctets) }}
                    </div>
                    <div class="text-sm text-gray-500">
                      ↑ {{ formatBytes(session.outputOctets) }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      :class="[
                        'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                        session.isActive
                          ? 'bg-green-100 text-green-800'
                          : 'bg-gray-100 text-gray-800'
                      ]"
                    >
                      {{ session.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <div class="flex justify-end space-x-2">
                      <UiButton
                        variant="secondary"
                        size="sm"
                        @click="viewSession(session)"
                      >
                        View
                      </UiButton>
                      <UiButton
                        variant="danger"
                        size="sm"
                        @click="disconnectSession(session)"
                        :loading="sessionsStore.loading"
                      >
                        Disconnect
                      </UiButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="sessionsStore.pagination.totalPages > 1" class="bg-white px-4 py-3 border-t border-gray-200 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex-1 flex justify-between sm:hidden">
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="sessionsStore.pagination.page === 1"
                  @click="goToPage(sessionsStore.pagination.page - 1)"
                >
                  Previous
                </UiButton>
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="sessionsStore.pagination.page === sessionsStore.pagination.totalPages"
                  @click="goToPage(sessionsStore.pagination.page + 1)"
                >
                  Next
                </UiButton>
              </div>
              <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p class="text-sm text-gray-700">
                    Showing
                    <span class="font-medium">{{ (sessionsStore.pagination.page - 1) * sessionsStore.pagination.pageSize + 1 }}</span>
                    to
                    <span class="font-medium">
                      {{ Math.min(sessionsStore.pagination.page * sessionsStore.pagination.pageSize, sessionsStore.pagination.total) }}
                    </span>
                    of
                    <span class="font-medium">{{ sessionsStore.pagination.total }}</span>
                    results
                  </p>
                </div>
                <div>
                  <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="sessionsStore.pagination.page === 1"
                      @click="goToPage(sessionsStore.pagination.page - 1)"
                    >
                      Previous
                    </UiButton>
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="sessionsStore.pagination.page === sessionsStore.pagination.totalPages"
                      @click="goToPage(sessionsStore.pagination.page + 1)"
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
            <SignalIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">No active sessions</h3>
            <p class="mt-1 text-sm text-gray-500">
              There are currently no active RADIUS sessions.
            </p>
            <div class="mt-6">
              <UiButton
                variant="primary"
                @click="refreshSessions"
              >
                <ArrowPathIcon class="w-4 h-4 mr-2" />
                Refresh
              </UiButton>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Session Details Modal -->
    <FormsSessionsDetailModal
      v-if="showDetailModal"
      :session="selectedSession"
      @close="showDetailModal = false"
      @disconnected="handleSessionDisconnected"
    />

    <!-- Session Stats Modal -->
    <FormsSessionsStatsModal
      v-if="showSessionStats"
      @close="showSessionStats = false"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  ArrowPathIcon, 
  ExclamationTriangleIcon, 
  XMarkIcon,
  SignalIcon,
  ChartBarIcon
} from '@heroicons/vue/24/outline'

definePageMeta({ 
  middleware: ['auth', 'role'],
  layout: 'default',
  meta: { roles: ['Admin', 'Manager'] } 
})

const sessionsStore = useSessionsStore()
const showDetailModal = ref(false)
const showSessionStats = ref(false)
const selectedSession = ref(null)

// Load sessions on mount
onMounted(async () => {
  await sessionsStore.fetchActiveSessions()
})

const viewSession = async (session: any) => {
  selectedSession.value = session
  showDetailModal.value = true
}

const disconnectSession = async (session: any) => {
  if (confirm(`Are you sure you want to disconnect the session for ${session.username}?`)) {
    const result = await sessionsStore.disconnectSession(session.sessionId, 'Administrative disconnect')
    if (result.success) {
      // Session disconnected successfully
    }
  }
}

const refreshSessions = async () => {
  await sessionsStore.fetchActiveSessions()
}

const goToPage = async (page: number) => {
  sessionsStore.setPage(page)
  await sessionsStore.fetchActiveSessions()
}

const handleSessionDisconnected = () => {
  showDetailModal.value = false
  // Refresh the sessions list
  sessionsStore.fetchActiveSessions()
}

// Utility functions
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const formatRelativeTime = (dateString: string) => {
  const now = new Date()
  const date = new Date(dateString)
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000)
  
  if (diffInSeconds < 60) return `${diffInSeconds}s ago`
  if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`
  if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`
  return `${Math.floor(diffInSeconds / 86400)}d ago`
}

const formatDuration = (seconds: number) => {
  if (seconds < 60) return `${seconds}s`
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ${seconds % 60}s`
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ${Math.floor((seconds % 3600) / 60)}m`
  return `${Math.floor(seconds / 86400)}d ${Math.floor((seconds % 86400) / 3600)}h`
}

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>
