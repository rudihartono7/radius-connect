<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <div class="flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">Audit Logs</h1>
            <p class="mt-1 text-sm text-gray-600">
              Monitor and analyze system activity and security events
            </p>
          </div>
          <div class="flex space-x-3">
            <UiButton
              variant="secondary"
              @click="showFilters = !showFilters"
            >
              <FunnelIcon class="w-4 h-4 mr-2" />
              Filters
            </UiButton>
            <UiButton
              variant="secondary"
              @click="showExportModal = true"
            >
              <ArrowDownTrayIcon class="w-4 h-4 mr-2" />
              Export
            </UiButton>
            <UiButton
              variant="primary"
              @click="showStatsModal = true"
            >
              <ChartBarIcon class="w-4 h-4 mr-2" />
              Statistics
            </UiButton>
          </div>
        </div>
      </div>

      <!-- Filters Panel -->
      <div v-if="showFilters" class="px-4 sm:px-0 mb-6">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <UiInput
              v-model="filters.actor"
              label="Actor"
              type="text"
              placeholder="Filter by actor name"
            />
            <UiInput
              v-model="filters.entity"
              label="Entity"
              type="text"
              placeholder="Filter by entity type"
            />
            <UiInput
              v-model="filters.action"
              label="Action"
              type="text"
              placeholder="Filter by action"
            />
            <UiInput
              v-model="filters.startDate"
              label="Start Date"
              type="date"
            />
            <UiInput
              v-model="filters.endDate"
              label="End Date"
              type="date"
            />
            <div class="flex items-end space-x-2">
              <UiInput
                v-model="filters.search"
                label="Search"
                type="text"
                placeholder="Search in logs..."
                class="flex-1"
              />
              <UiButton
                variant="secondary"
                @click="applyFilters"
                :loading="auditStore.loading"
              >
                Apply
              </UiButton>
              <UiButton
                variant="outline"
                @click="clearFilters"
              >
                Clear
              </UiButton>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Display -->
      <div v-if="auditStore.error" class="mb-6 px-4 sm:px-0">
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ auditStore.error }}</p>
            </div>
            <div class="ml-auto pl-3">
              <button
                @click="auditStore.clearError()"
                class="text-red-400 hover:text-red-600"
              >
                <XMarkIcon class="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="auditStore.loading && !auditStore.hasLogs" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          <div class="flex justify-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
          <p class="text-center mt-4 text-gray-600">Loading audit logs...</p>
        </div>
      </div>

      <!-- Audit Logs Table -->
      <div v-else-if="auditStore.hasLogs" class="px-4 sm:px-0">
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Timestamp
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actor
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Action
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Entity
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Severity
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    IP Address
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="log in auditStore.logs" :key="log.id" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ formatDate(log.timestamp) }}
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ formatRelativeTime(log.timestamp) }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <div class="flex-shrink-0 h-8 w-8">
                        <div class="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
                          <span class="text-sm font-medium text-blue-600">
                            {{ log.actorId }}
                          </span>
                        </div>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">
                          {{ log.actorName }}
                        </div>
                        <div class="text-sm text-gray-500">
                          {{ log.actorId }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ log.action }}
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ log.category }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900">
                      {{ log.entityType }}
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ log.entityId }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      :class="[
                        'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                        getSeverityColor(log.severity)
                      ]"
                    >
                      {{ log.severity }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ log.ipAddress }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <UiButton
                      variant="secondary"
                      size="sm"
                      @click="viewLog(log)"
                    >
                      View
                    </UiButton>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="auditStore.pagination.totalPages > 1" class="bg-white px-4 py-3 border-t border-gray-200 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex-1 flex justify-between sm:hidden">
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="auditStore.pagination.page === 1"
                  @click="goToPage(auditStore.pagination.page - 1)"
                >
                  Previous
                </UiButton>
                <UiButton
                  variant="secondary"
                  size="sm"
                  :disabled="auditStore.pagination.page === auditStore.pagination.totalPages"
                  @click="goToPage(auditStore.pagination.page + 1)"
                >
                  Next
                </UiButton>
              </div>
              <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p class="text-sm text-gray-700">
                    Showing
                    <span class="font-medium">{{ (auditStore.pagination.page - 1) * auditStore.pagination.pageSize + 1 }}</span>
                    to
                    <span class="font-medium">
                      {{ Math.min(auditStore.pagination.page * auditStore.pagination.pageSize, auditStore.pagination.total) }}
                    </span>
                    of
                    <span class="font-medium">{{ auditStore.pagination.total }}</span>
                    results
                  </p>
                </div>
                <div>
                  <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="auditStore.pagination.page === 1"
                      @click="goToPage(auditStore.pagination.page - 1)"
                    >
                      Previous
                    </UiButton>
                    <UiButton
                      variant="secondary"
                      size="sm"
                      :disabled="auditStore.pagination.page === auditStore.pagination.totalPages"
                      @click="goToPage(auditStore.pagination.page + 1)"
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
            <DocumentTextIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">No audit logs</h3>
            <p class="mt-1 text-sm text-gray-500">
              No audit logs found matching your criteria.
            </p>
            <div class="mt-6">
              <UiButton
                variant="primary"
                @click="refreshLogs"
              >
                <ArrowPathIcon class="w-4 h-4 mr-2" />
                Refresh
              </UiButton>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Log Details Modal -->
    <AuditLogDetailModal
      v-if="showDetailModal"
      :log="selectedLog"
      @close="showDetailModal = false"
    />

    <!-- Export Modal -->
    <AuditExportModal
      v-if="showExportModal"
      @close="showExportModal = false"
      @export="handleExport"
    />

    <!-- Statistics Modal -->
    <AuditStatsModal
      v-if="showStatsModal"
      @close="showStatsModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  FunnelIcon, 
  ArrowDownTrayIcon, 
  ChartBarIcon,
  ExclamationTriangleIcon, 
  XMarkIcon,
  DocumentTextIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'

definePageMeta({ 
  middleware: ['auth', 'role'],
  layout: 'default',
  meta: { roles: ['Admin', 'Manager'] } 
})

const auditStore = useAuditStore()
const showFilters = ref(false)
const showDetailModal = ref(false)
const showExportModal = ref(false)
const showStatsModal = ref(false)
const selectedLog = ref(null)

const filters = reactive<{
  actor: string
  entity: string
  action: string
  startDate: string
  endDate: string
  search: string
  severity: string
}>({
  actor: '',
  entity: '',
  action: '',
  startDate: '',
  endDate: '',
  search: '',
  severity: ''
})

// Load logs on mount
onMounted(async () => {
  await auditStore.fetchAuditLogs()
})

const viewLog = async (log: any) => {
  selectedLog.value = log
  showDetailModal.value = true
}

const applyFilters = async () => {
  auditStore.setFilters(filters)
  await auditStore.fetchAuditLogs()
  console.log('has logs', auditStore.logs.length)
}

const clearFilters = async () => {
  Object.keys(filters).forEach(key => {
    (filters as any)[key] = ''
  })
  auditStore.clearFilters()
  await auditStore.fetchAuditLogs()
}

const refreshLogs = async () => {
  await auditStore.fetchAuditLogs()
}

const goToPage = async (page: number) => {
  auditStore.setPage(page)
  await auditStore.fetchAuditLogs()
}

const handleExport = async (params: any) => {
  showExportModal.value = false
  if (params.format === 'csv') {
    await auditStore.exportToCsv(params)
  } else if (params.format === 'json') {
    await auditStore.exportToJson(params)
  }
}

const getSeverityColor = (severity: string) => {
  switch (severity) {
    case 'Critical':
      return 'bg-red-100 text-red-800'
    case 'High':
      return 'bg-orange-100 text-orange-800'
    case 'Medium':
      return 'bg-yellow-100 text-yellow-800'
    case 'Low':
      return 'bg-green-100 text-green-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
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
</script>
