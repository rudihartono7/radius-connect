<template>
  <div class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
      <div class="mt-3">
        <!-- Header -->
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-medium text-gray-900">Disconnect Session</h3>
          <button
            @click="$emit('close')"
            class="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Session: {{ session?.username }}
            </label>
            <p class="text-sm text-gray-500 mb-4">
              Session ID: {{ session?.sessionId?.substring(0, 8) }}...
            </p>
          </div>

          <UiInput
            v-model="form.reason"
            label="Disconnect Reason"
            type="text"
            required
            :error="errors.reason"
            placeholder="Enter reason for disconnection"
          />

          <!-- Error Display -->
          <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-3">
            <p class="text-sm text-red-700">{{ error }}</p>
          </div>

          <!-- Actions -->
          <div class="flex justify-end space-x-3 pt-4">
            <UiButton
              type="button"
              variant="secondary"
              @click="$emit('close')"
            >
              Cancel
            </UiButton>
            <UiButton
              type="submit"
              variant="danger"
              :loading="loading"
            >
              Disconnect
            </UiButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { XMarkIcon } from '@heroicons/vue/24/outline'

interface Props {
  session: any
}

const props = defineProps<Props>()
const emit = defineEmits<{ close: []; disconnected: [] }>()

const sessionsStore = useSessionsStore()

const form = reactive({
  reason: ''
})

const errors = reactive({
  reason: ''
})

const error = ref('')
const loading = ref(false)

const validateForm = () => {
  errors.reason = ''
  
  if (!form.reason.trim()) {
    errors.reason = 'Reason is required'
  }
  
  return !Object.values(errors).some(error => error)
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  loading.value = true
  error.value = ''

  try {
    const result = await sessionsStore.disconnectSession(
      props.session.sessionId,
      form.reason.trim()
    )

    if (result.success) {
      emit('disconnected')
    } else {
      error.value = result.error || 'Failed to disconnect session'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}
</script>
