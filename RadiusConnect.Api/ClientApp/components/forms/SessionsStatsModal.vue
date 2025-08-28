<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <h3 class="text-xl font-medium text-gray-900">Session Statistics</h3>
            <p class="text-sm text-gray-600 mt-1">
              Overview of RADIUS session statistics and metrics
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
          <!-- Overview Stats -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-blue-50 rounded-lg p-4">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <SignalIcon class="h-8 w-8 text-blue-600" />
                </div>
                <div class="ml-4">
                  <p class="text-sm font-medium text-blue-600">Active Sessions</p>
                  <p class="text-2xl font-semibold text-blue-900">{{ stats.activeSessions || 0 }}</p>
                </div>
              </div>
            </div>

            <div class="bg-green-50 rounded-lg p-4">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <ClockIcon class="h-8 w-8 text-green-600" />
                </div>
                <div class="ml-4">
                  <p class="text-sm font-medium text-green-600">Avg Session Time</p>
                  <p class="text-2xl font-semibold text-green-900">{{ formatDuration(stats.averageSessionTime || 0) }}</p>
                </div>
              </div>
            </div>

            <div class="bg-purple-50 rounded-lg p-4">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <ArrowUpIcon class="h-8 w-8 text-purple-600" />
                </div>
                <div class="ml-4">
                  <p class="text-sm font-medium text-purple-600">Total Data Transfer</p>
                  <p class="text-2xl font-semibold text-purple-900">{{ formatBytes(stats.totalDataTransfer || 0) }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Session Distribution -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Session Distribution by NAS</h4>
            <div v-if="stats.nasDistribution && stats.nasDistribution.length > 0" class="space-y-3">
              <div v-for="nas in stats.nasDistribution" :key="nas.nasIp" class="bg-white rounded border p-3">
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-sm font-medium text-gray-900">{{ nas.nasIp }}</p>
                    <p class="text-sm text-gray-500">{{ nas.nasName || 'Unknown' }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-sm font-medium text-gray-900">{{ nas.sessionCount }} sessions</p>
                    <p class="text-sm text-gray-500">{{ formatBytes(nas.totalDataTransfer || 0) }}</p>
                  </div>
                </div>
              </div>
            </div>
            <div v-else class="text-center py-4">
              <p class="text-sm text-gray-500">No session distribution data available</p>
            </div>
          </div>

          <!-- Recent Activity -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Recent Session Activity</h4>
            <div v-if="stats.recentActivity && stats.recentActivity.length > 0" class="space-y-3">
              <div v-for="activity in stats.recentActivity" :key="activity.id" class="bg-white rounded border p-3">
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-sm font-medium text-gray-900">{{ activity.username }}</p>
                    <p class="text-sm text-gray-500">{{ activity.action }} - {{ formatRelativeTime(activity.timestamp) }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-sm text-gray-500">{{ activity.nasIp }}</p>
                  </div>
                </div>
              </div>
            </div>
            <div v-else class="text-center py-4">
              <p class="text-sm text-gray-500">No recent activity data available</p>
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
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  SignalIcon,
  ClockIcon,
  ArrowUpIcon
} from '@heroicons/vue/24/outline'

const emit = defineEmits<{ close: [] }>()

const loading = ref(false)
const stats = ref({
  activeSessions: 0,
  averageSessionTime: 0,
  totalDataTransfer: 0,
  nasDistribution: [] as Array<{ nasIp: string; nasName: string; sessionCount: number; totalDataTransfer: number }>,
  recentActivity: [] as Array<{ id: number; username: string; action: string; timestamp: string; nasIp: string }>
})

// Load stats on mount
onMounted(async () => {
  await loadStats()
})

const loadStats = async () => {
  loading.value = true
  try {
    // Mock data for now - replace with actual API call
    stats.value = {
      activeSessions: 12,
      averageSessionTime: 3600, // 1 hour in seconds
      totalDataTransfer: 1024 * 1024 * 1024 * 5, // 5 GB
      nasDistribution: [
        { nasIp: '192.168.1.1', nasName: 'Main Router', sessionCount: 5, totalDataTransfer: 1024 * 1024 * 1024 * 2 },
        { nasIp: '192.168.1.2', nasName: 'Secondary Router', sessionCount: 3, totalDataTransfer: 1024 * 1024 * 1024 * 1.5 },
        { nasIp: '192.168.1.3', nasName: 'Backup Router', sessionCount: 4, totalDataTransfer: 1024 * 1024 * 1024 * 1.5 }
      ],
      recentActivity: [
        { id: 1, username: 'user1', action: 'Connected', timestamp: new Date(Date.now() - 300000).toISOString(), nasIp: '192.168.1.1' },
        { id: 2, username: 'user2', action: 'Disconnected', timestamp: new Date(Date.now() - 600000).toISOString(), nasIp: '192.168.1.2' },
        { id: 3, username: 'user3', action: 'Connected', timestamp: new Date(Date.now() - 900000).toISOString(), nasIp: '192.168.1.1' }
      ]
    }
  } catch (error) {
    console.error('Failed to load stats:', error)
  } finally {
    loading.value = false
  }
}

// Utility functions
const formatDuration = (seconds: number) => {
  if (seconds < 60) return `${seconds}s`
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m`
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h`
  return `${Math.floor(seconds / 86400)}d`
}

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
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
