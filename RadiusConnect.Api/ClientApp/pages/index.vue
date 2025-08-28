<template>
  <div class="min-h-screen bg-gray-50">
    <LayoutHeader />
    
    <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 sm:px-0 mb-8">
        <h1 class="text-2xl font-semibold text-gray-900">Dashboard</h1>
        <p class="mt-1 text-sm text-gray-600">
          Overview of your RADIUS system and network activity
        </p>
      </div>

      <!-- Loading State -->
      <div v-if="dashboardStore.loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="dashboardStore.error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <div class="flex">
          <div class="flex-shrink-0">
            <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-800">Error loading dashboard data</h3>
            <p class="mt-1 text-sm text-red-700">{{ dashboardStore.error }}</p>
          </div>
        </div>
      </div>

      <!-- Dashboard Content -->
      <div v-else-if="dashboardStore.hasData" class="space-y-6">
        <!-- Overview Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <MetricCard
            title="Total Users"
            :value="dashboardStore.overview?.totalUsers || 0"
            icon="UsersIcon"
            trend="+12%"
            trendDirection="up"
          />
          <MetricCard
            title="Active Sessions"
            :value="dashboardStore.overview?.activeSessions || 0"
            icon="SignalIcon"
            trend="+5%"
            trendDirection="up"
          />
          <MetricCard
            title="Total Groups"
            :value="dashboardStore.overview?.totalGroups || 0"
            icon="UserGroupIcon"
            trend="+2%"
            trendDirection="up"
          />
          <MetricCard
            title="Success Rate"
            :value="`${Math.round((dashboardStore.overview?.successRate || 0) * 100)}%`"
            icon="CheckCircleIcon"
            trend="+3%"
            trendDirection="up"
          />
        </div>

        <!-- System Health -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">System Health</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p class="text-sm font-medium text-gray-600">Database</p>
                <p class="text-lg font-semibold text-gray-900">
                  {{ dashboardStore.systemHealth?.databaseStatus || 'Unknown' }}
                </p>
              </div>
              <div class="w-3 h-3 rounded-full" :class="getStatusColor(dashboardStore.systemHealth?.databaseStatus)"></div>
            </div>
            <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p class="text-sm font-medium text-gray-600">RADIUS Server</p>
                <p class="text-lg font-semibold text-gray-900">
                  {{ dashboardStore.systemHealth?.radiusServerStatus || 'Unknown' }}
                </p>
              </div>
              <div class="w-3 h-3 rounded-full" :class="getStatusColor(dashboardStore.systemHealth?.radiusServerStatus)"></div>
            </div>
            <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p class="text-sm font-medium text-gray-600">CPU Usage</p>
                <p class="text-lg font-semibold text-gray-900">
                  {{ Math.round(dashboardStore.systemHealth?.cpuUsage || 0) }}%
                </p>
              </div>
              <div class="w-3 h-3 rounded-full" :class="getUsageColor(dashboardStore.systemHealth?.cpuUsage)"></div>
            </div>
          </div>
        </div>

        <!-- Charts Row -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Session Statistics -->
          <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h2 class="text-lg font-medium text-gray-900 mb-4">Session Statistics</h2>
            <div class="space-y-4">
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Total Sessions</span>
                <span class="font-semibold">{{ dashboardStore.sessionStats?.totalSessions || 0 }}</span>
              </div>
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Active Sessions</span>
                <span class="font-semibold">{{ dashboardStore.sessionStats?.activeSessions || 0 }}</span>
              </div>
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Average Duration</span>
                <span class="font-semibold">{{ formatDuration(dashboardStore.sessionStats?.averageDuration || 0) }}</span>
              </div>
            </div>
          </div>

          <!-- Authentication Statistics -->
          <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h2 class="text-lg font-medium text-gray-900 mb-4">Authentication Statistics</h2>
            <div class="space-y-4">
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Total Attempts</span>
                <span class="font-semibold">{{ dashboardStore.authStats?.totalAttempts || 0 }}</span>
              </div>
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Successful</span>
                <span class="font-semibold text-green-600">{{ dashboardStore.authStats?.successfulAttempts || 0 }}</span>
              </div>
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600">Failed</span>
                <span class="font-semibold text-red-600">{{ dashboardStore.authStats?.failedAttempts || 0 }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-12">
        <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <ChartBarIcon class="w-8 h-8 text-gray-400" />
        </div>
        <h3 class="text-lg font-medium text-gray-900 mb-2">No dashboard data available</h3>
        <p class="text-gray-600">Dashboard data will appear here once your system is configured.</p>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ExclamationTriangleIcon, ChartBarIcon } from '@heroicons/vue/24/outline'

definePageMeta({
  middleware: ['auth'],
  layout: 'default'
})

const dashboardStore = useDashboardStore()

// Fetch dashboard data on page load
onMounted(async () => {
  await dashboardStore.fetchAllData()
})

const getStatusColor = (status?: string) => {
  switch (status?.toLowerCase()) {
    case 'online':
    case 'healthy':
      return 'bg-green-500'
    case 'offline':
    case 'unhealthy':
      return 'bg-red-500'
    default:
      return 'bg-yellow-500'
  }
}

const getUsageColor = (usage?: number) => {
  if (!usage) return 'bg-gray-500'
  if (usage < 50) return 'bg-green-500'
  if (usage < 80) return 'bg-yellow-500'
  return 'bg-red-500'
}

const formatDuration = (seconds: number) => {
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  
  if (hours > 0) {
    return `${hours}h ${minutes}m`
  }
  return `${minutes}m`
}
</script>
