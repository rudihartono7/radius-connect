<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-10 mx-auto p-5 border w-4/5 max-w-4xl shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <h3 class="text-xl font-medium text-gray-900">
              Session Details
            </h3>
            <p class="text-sm text-gray-600 mt-1">
              {{ session?.username }} - {{ session?.sessionId?.substring(0, 8) }}...
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
        <div v-else-if="session" class="space-y-6">
          <!-- Session Information -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Session Information</h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Username</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.username }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Session ID</label>
                <p class="mt-1 text-sm text-gray-900 font-mono">{{ session.sessionId }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Status</label>
                <span
                  :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full mt-1',
                    session.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  ]"
                >
                  {{ session.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Framed IP Address</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.framedIpAddress || 'Not assigned' }}</p>
              </div>
            </div>
          </div>

          <!-- Network Information -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Network Information</h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">NAS IP Address</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.nasIpAddress }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">NAS Port</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.nasPort }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Called Station ID</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.calledStationId || 'Not specified' }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Calling Station ID</label>
                <p class="mt-1 text-sm text-gray-900">{{ session.callingStationId || 'Not specified' }}</p>
              </div>
            </div>
          </div>

          <!-- Timing Information -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Timing Information</h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Start Time</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatDate(session.startTime) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Last Update</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatDate(session.lastUpdate) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Session Duration</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatDuration(session.sessionTime) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Time Since Start</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatRelativeTime(session.startTime) }}</p>
              </div>
            </div>
          </div>

          <!-- Data Usage -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Data Usage</h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Input Octets (Download)</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatBytes(session.inputOctets) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Output Octets (Upload)</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatBytes(session.outputOctets) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Total Data Transfer</label>
                <p class="mt-1 text-sm text-gray-900">{{ formatBytes(session.inputOctets + session.outputOctets) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Average Speed</label>
                <p class="mt-1 text-sm text-gray-900">{{ calculateAverageSpeed(session) }}</p>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h4 class="text-lg font-medium text-gray-900 mb-4">Actions</h4>
            <div class="flex space-x-3">
              <UiButton
                variant="danger"
                @click="showDisconnectModal = true"
                :disabled="!session.isActive"
              >
                <SignalSlashIcon class="w-4 h-4 mr-2" />
                Disconnect Session
              </UiButton>
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

    <!-- Disconnect Modal -->
    <SessionsDisconnectModal
      v-if="showDisconnectModal"
      :session="session"
      @close="showDisconnectModal = false"
      @disconnected="handleSessionDisconnected"
    />
  </div>
</template>

<script setup lang="ts">
import { 
  XMarkIcon, 
  SignalSlashIcon 
} from '@heroicons/vue/24/outline'

interface Props {
  session: any
}

const props = defineProps<Props>()
const emit = defineEmits<{ close: []; disconnected: [] }>()

const loading = ref(false)
const showDisconnectModal = ref(false)

const handleSessionDisconnected = () => {
  showDisconnectModal.value = false
  emit('disconnected')
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

const calculateAverageSpeed = (session: any) => {
  if (session.sessionTime === 0) return '0 B/s'
  const totalBytes = session.inputOctets + session.outputOctets
  const bytesPerSecond = totalBytes / session.sessionTime
  return formatBytes(bytesPerSecond) + '/s'
}
</script>
